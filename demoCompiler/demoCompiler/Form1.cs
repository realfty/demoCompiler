using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;

namespace demoCompiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        private string inputProgram;
        private int inputPointer;
        private twoTuple presentInput;
        private identifierTable identifyTable;
        private MiddleCodeTable midTable;
        private tempVarTable temptable;
        private constTable consttable;
        private void print(string str)
        {
            listBox1.Items.Add(str);
        }
        private void print2(string str)
        {
            listBox2.Items.Add(str);
        }
        //��listBox1���ݱ��沢�����debug.txt��
        private void saveDebuglistBox1()
        {
            string path = "debug.txt";
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            foreach (string str in listBox1.Items)
            {
                sw.WriteLine(str);
            }
            sw.Close();
            fs.Close();
        }
        //��listBox2���ݱ��沢�����out.txt��
        private void saveOutlistBox2()
        {
            string path = "out.txt";
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            foreach (string str in listBox2.Items)
            {
                sw.WriteLine(str);
            }
            sw.Close();
            fs.Close();
        }
        //�����봮���д�ӡ�����ֺ�ת�У����������������
        private void printInputProgram(string str)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ';')
                {
                    print(str.Substring(count, i - count + 1));
                    count = i + 1;
                }
                if (str[i] == '{')
                {
                    print(str.Substring(count, i - count + 1));
                    count = i + 1;
                    print("    ");
                }
                if (str[i] == '}')
                {
                    print(str.Substring(count, i - count + 1));
                    count = i + 1;
                    print("    ");
                }
                if (str[i] == ')'&& str[i+1] != ';')
                {
                    print(str.Substring(count, i - count + 1));
                    count = i + 1;
                }
                
            }
        }
        //10��������ֹ
        private void killProcess(Process p)
        {
            Thread.Sleep(10000);
            p.Kill();
        }
        private class twoTuple
        {
            public string type { get; set; }
            public string value { get; set; }
            public twoTuple(string type, string value)
            {
                this.type = type;
                this.value = value;
            }
            public override string ToString()
            {
                return "( " + this.type + " , " + this.value + " )";
            }

        }
        private class fourTuple
        {
            public string op1 { get; set; }
            public string op2 { get; set; }
            public string op3 { get; set; }
            public string op4 { get; set; }
            public fourTuple(string op1, string op2, string op3, string op4)
            {
                this.op1 = op1;
                this.op2 = op2;
                this.op3 = op3;
                this.op4 = op4;
            }
            public override string ToString()
            {
                return "( " + this.op1 + " , " + this.op2 + " , " + this.op3 + " , " + this.op4 + " )";
            }
            public void updateOP4(string op4)
            {
                this.op4 = op4;
            }
        }

        private class MiddleCodeTable
        {
            private List<fourTuple> table;
            public int NXQ { get { return table.Count; } }

            public MiddleCodeTable()
            {
                table = new List<fourTuple>();
            }
            public bool Add(string op1, string op2, string op3, string op4)
            {
                table.Add(new fourTuple(op1, op2, op3, op4));
                return true;

            }
            public void tablePrint(ListBox lb)
            {
                lb.Items.Add("��������������Ԫʽ������������");
                for (int i = 0; i < table.Count; i++)
                {
                    lb.Items.Add(" ( " + i + " ) " + table[i].ToString());
                }

            }
            public bool backpath(int index, string op4)
            {
                table[index].updateOP4(op4);

                return true;
            }
        }

        private class identifier
        {
            public string name { get; set; }
            public string type { get; set; }
            public string value { get; set; }
            public identifier(string name)
            {
                this.name = name;
                this.type = "";
                this.value = "";
            }
            public override string ToString()
            {
                string result = "( " + this.name + ", ";
                if (this.type == "")
                {
                    result += "[null], ";
                }
                else
                {
                    result += this.type + ", ";
                }
                if (this.value == "")
                {
                    result += "[null] )";
                }
                else
                {
                    result += this.value + " )";
                }


                return result;
            }
            public string getType(string name)
            {
                if (this.name == name)
                {
                    return this.type;
                }
                else
                {
                    return "";
                }

            }
            public string getValue(string name)
            {
                if (this.name == name)
                {
                    return this.value;
                }
                else
                {
                    return "";
                }
            }

            

        }
        private class identifierTable
        {
            private List<identifier> table;
            public identifierTable()
            {
                table = new List<identifier>();
            }
            public bool Exist(string name)
            {
                foreach (identifier id in table)
                {
                    if (id.name == name)
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool Add(string name)
            {
                if (Exist(name))
                {
                    return false;
                }

                table.Add(new identifier(name));
                return true;
            }
            public void tablePrint(ListBox lb)
            {
                lb.Items.Add("����������������ʶ���������ͣ�ֵ��������������");
                foreach (identifier id in table)
                {
                    lb.Items.Add(id.ToString());
                }
            }

            private int getIndex(string name)
            {
                for (int i = 0; i < table.Count; i++)
                {
                    if (table[i].name == name)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public bool UpdateType(string name, string type)
            {
                int index = this.getIndex(name);
                if (index == -1)
                {
                    return false;
                }
                table[index].type = type;
                return true;
            }
            public bool UpdateValue(string name, string value)
            {
                int index = this.getIndex(name);
                if (index == -1)
                {
                    return false;
                }
                table[index].value = value;
                return true;
            }
            public string getValue(string name)
            {
                foreach (identifier id in table)
                {
                    if (id.name == name)
                    {
                        return id.value;
                    }
                }
                return "";
            }
            
            public string getType(string name)
            {
                foreach (identifier id in table)
                {
                    if (id.name == name)
                    {
                        return id.type;
                    }
                }
                return "";
            }

            public identifier getIdentifier(string name)
            {
                foreach (identifier id in table)
                {
                    if (id.name == name)
                    {
                        return id;
                    }
                }
                return null;
            }

            
            
        }
        private class constTable
        {
            private List<twoTuple> table;
            public constTable()
            {
                table = new List<twoTuple>();
            }
            public twoTuple newConst(string type, string value)
            {
                twoTuple newConst = new twoTuple(type, value);
                //�ж���������ֵ�Ƿ��ظ�
                foreach (twoTuple t in table)
                {
                    if (t.type == type && t.value == value)
                    {
                        return t;
                    }
                }
                table.Add(newConst);
                return newConst;
            }
            public bool Exist(string value)
            {
                foreach (twoTuple t in table)
                {
                    if (t.value == value)
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool Add(string type, string value)
            {
                if (Exist(value))
                {
                    return false;
                }
                table.Add(new twoTuple(type, value));
                return true;
            }
            public void tablePrint(ListBox lb)
            {
                lb.Items.Add("���������������������ͣ���Ӧֵ��������������");
                foreach (twoTuple t in table)
                {
                    lb.Items.Add(t.ToString());
                }
            }
            public string getType(string value)
            {
                foreach (twoTuple t in table)
                {
                    if (t.value == value)
                    {
                        return t.type;
                    }
                }
                return "";
            }
        }

        private class tempVarTable
        {
            private int count = 1;
            private List<string> table;
            public tempVarTable()
            {
                table = new List<string>();
            }
            public identifier tempVar()
            {
                int index = count++;
                identifier t = new identifier("T" + index.ToString());
                
                //������ʱ����
                table.Add(t.name);
                table.Add(t.type);
                table.Add(t.value);
                return t;
            }
            //����type��value
            public void saveTempVar(string name, string type, string value)
            {
                int index = table.IndexOf(name);
                table[index + 1] = type;
                table[index + 2] = value;
            }

            public bool Exist(string name)
            {
                foreach (string id in table)
                {
                    if (id == name)
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool Add(string name)
            {
                if (Exist(name))
                {
                    return false;
                }

                table.Add(name);
                return true;
            }
            public bool updateType(string name, string type)
            {
                int index = table.IndexOf(name);
                if (index == -1)
                {
                    return false;
                }
                table[index + 1] = type;
                return true;
            }
            public bool updateValue(string name, string value)
            {
                int index = table.IndexOf(name);
                if (index == -1)
                {
                    return false;
                }
                table[index + 2] = value;
                return true;
            }
            

            public void tablePrint(ListBox lb)
            {
                lb.Items.Add("��������������ʱ����������������");
                //������ӡ������ʱ�����Ͷ�Ӧtype��value,��ӡ��һ����
                for (int i = 0; i < table.Count; i += 3)
                {
                    lb.Items.Add(" ( " + table[i] + ",  " + table[i + 1] + ",  " + table[i + 2] + "  )");
                }

                
            }
        }
        

        //�ʷ���������
        private twoTuple nextInput()
        {
            string tempWord = "";
            int state = 0;
            while (inputProgram[inputPointer] != '#')
            {
                char symbol = inputProgram[inputPointer];
                if (symbol == ' ') { inputPointer++; continue; }
                if (state == 0)
                {
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        if (symbol == 's') { state = 0201; tempWord += symbol; inputPointer++; continue; }
                        if (symbol == 'e') { state = 0301; tempWord += symbol; inputPointer++; continue; }
                        if (symbol == 'i') { state = 0401; tempWord += symbol; inputPointer++; continue; }
                        if (symbol == 'd') { state = 1001; tempWord += symbol; inputPointer++; continue; }
                        if (symbol == 'w') { state = 1101; tempWord += symbol; inputPointer++; continue; }
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    if (symbol == '=') { state = 1201; tempWord += symbol; inputPointer++; continue; }
                    if (symbol == '"')
                    {
                        { state = 0701; //tempWord += symbol;
                          inputPointer++; continue; }
                    }
                    if (symbol == '<')
                    {
                        { state = 0801; tempWord += symbol; inputPointer++; continue; }
                    }
                    if (symbol == '>')
                    {
                        { state = 0901; tempWord += symbol; inputPointer++; continue; }
                    }
                    if (symbol == ';')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("�ֺ�", tempWord); }
                    }
                    if (symbol == ',')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("����", tempWord); }
                    }
                    if (symbol == '(')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("������", tempWord); }
                    }
                    if (symbol == ')')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("������", tempWord); }
                    }
                    if (symbol == '+')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("���Ӻ�", tempWord); }
                    }
                    if (symbol == '*')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("�ظ������", tempWord); }
                    }
                    if (symbol >= '0' && symbol <= '9') { state = 13; tempWord += symbol; inputPointer++; continue; }

                }
                if (state == 1201)
                {
                    if (symbol == '=')
                    {
                        tempWord += symbol; inputPointer++; return new twoTuple("���ں�", tempWord);
                    }
                    return new twoTuple("��ֵ��", tempWord);
                }
                if (state == 0701)
                {
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        { state = 0701; tempWord += symbol; inputPointer++; continue; }
                    }
                    if (symbol == '"')
                    {
                        { //tempWord += symbol;
                          inputPointer++; return new twoTuple("�ַ���", tempWord); }
                    }
                }

                if (state == 0801)
                {
                    //<>
                    if (symbol == '>') { tempWord += symbol; inputPointer++; return new twoTuple("������", tempWord); }
                    //<
                    if (symbol == '=') { tempWord += symbol; inputPointer++; return new twoTuple("С�ڵ���", tempWord); }
                    return new twoTuple("С��", tempWord);

                }
                if (state == 0901)
                {
                    //=
                    if (symbol == '=') { tempWord += symbol; inputPointer++; return new twoTuple("���ڵ���", tempWord); }
                    //
                    return new twoTuple("����", tempWord);
                }
                if (state == 0401)
                {
                    if (symbol == 'f') { tempWord += symbol; inputPointer++; return new twoTuple("�ؼ���if", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0201)
                {
                    if (symbol == 't') { state = 0202; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0202)
                {
                    if (symbol == 'r') { state = 0203; tempWord += symbol; inputPointer++; continue; }
                    if (symbol == 'a') { state = 0603; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0203)
                {
                    if (symbol == 'i') { state = 0204; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0204)
                {
                    if (symbol == 'n') { state = 0205; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0205)
                {
                    if (symbol == 'g') { tempWord += symbol; inputPointer++; return new twoTuple("�ؼ���string", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0603)
                {
                    if (symbol == 'r') { state = 0604; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0604)
                {
                    if (symbol == 't') { tempWord += symbol; inputPointer++; return new twoTuple("�ؼ���start", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0301)
                {
                    if (symbol == 'l') { state = 0302; tempWord += symbol; inputPointer++; continue; }
                    if (symbol == 'n') { state = 0502; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0302)
                {
                    if (symbol == 's') { state = 0303; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0303)
                {
                    if (symbol == 'e') { tempWord += symbol; inputPointer++; return new twoTuple("�ؼ���else", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0502)
                {
                    if (symbol == 'd') { tempWord += symbol; inputPointer++; return new twoTuple("�ؼ���end", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 1001)
                {
                    if (symbol == 'o') { tempWord += symbol; inputPointer++; return new twoTuple("�ؼ���do", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 1101)
                {
                    if (symbol == 'h') { state = 1102; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 1102)
                {
                    if (symbol == 'i') { state = 1103; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 1103)
                {
                    if (symbol == 'l') { state = 1104; tempWord += symbol; inputPointer++; continue; }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 1104)
                {
                    if (symbol == 'e') { tempWord += symbol; inputPointer++; return new twoTuple("�ؼ���while", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 1)
                {
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        tempWord += symbol; inputPointer++; continue;
                    }
                    if (symbol >= '0' && symbol <= '9')
                    {
                        tempWord += symbol; inputPointer++; continue;
                    }
                    identifyTable.Add(tempWord);
                    return new twoTuple("��ʶ��", tempWord);
                }
                if (state == 13)
                {
                    if (symbol >= '0' && symbol <= '9')
                    {
                        tempWord += symbol; inputPointer++; continue;
                    }
                    /*if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }*/
                    if (symbol == ' ') { tempWord += symbol; inputPointer++; return new twoTuple("����", tempWord); }
                    return new twoTuple("����", tempWord);
                }
                state = 0;
                tempWord = "";
                inputPointer++;
            }
            return new twoTuple("#", "#");


        }
        //ƥ�亯��
        private bool match(string type)
        {
            if (type == presentInput.type)
            {
                print("[�﷨]��" + type + "���ͷ��" + presentInput.type + "��ͬ��ƥ��ɹ�");
                presentInput = nextInput();
                print("[�ʷ�]��ʶ����һ������Ϊ" + presentInput.ToString());
                return true;
            }
            print("[�﷨]������" + type + "���ͷ��" + presentInput.type + "����ͬ��ƥ��ʧ��");
            //��ӡ����λ��
            print("[�ʷ�]������λ��Ϊ" + presentInput.ToString());
            //ɾ�����󵥴�
            presentInput.type = "#";
            presentInput.value = "#";
            


            return false;
        }
        private bool P()
        {
            //P -> string I; D
            print("[�﷨]�Ƶ���<����> -> string <��ʶ���б�>; <��䲿��>");
            match("�ؼ���string");
            I("string");
            match("�ֺ�");
            D();
            return true;
        }
        private bool I(string type)
        {
            //I -> i I'
            print("[�﷨]�Ƶ���<��ʶ���б�> -> ��ʶ��,<��ʶ���б�'>");
            string name = presentInput.value;
            match("��ʶ��");
            identifyTable.UpdateType(name, type);
            Ip(type);
            return true;
        }
        private bool Ip(string type)
        {
            //I' -> ,i I' | ��
            if (presentInput.type == "����")
            {
                //I' -> ,i I' 
                print("[�﷨]�Ƶ���<��ʶ���б�'> -> ,<��ʶ���б�><��ʶ���б�'>");
                match("����");
                string name = presentInput.value;
                match("��ʶ��");
                identifyTable.UpdateType(name, type);
                Ip(type);
                return true;
            }
            if (presentInput.type == "�ֺ�")
            {
                //I' -> ��
                print("[�﷨]�Ƶ���<��ʶ���б�'> -> ��");
                return true;
            }
            return false;
        }
        private bool D()
        {
            //D -> S; D'
            print("[�﷨]�Ƶ���<��䲿��> -> <���>;<��䲿��'>");
            S();
            match("�ֺ�");
            Dp();
            return true;
        }
        private bool S()
        {
            //S -> A|C|L
            if (presentInput.type == "��ʶ��")
            {
                print("[�﷨]�Ƶ���<���> -> <��ֵ���>");
                A();
                return true;
            }
            if (presentInput.type == "�ؼ���if")
            {
                print("[�﷨]�Ƶ���<���> -> <�������>");
                C();
                return true;
            }
            if (presentInput.type == "�ؼ���do")
            {
                print("[�﷨]�Ƶ���<���> -> <ѭ�����>");
                L();
                return true;
            }
            return false;
        }
        private bool Dp()
        {
            //D' -> S; D' | ��

            if (presentInput.type == "��ʶ��" || presentInput.type == "�ؼ���if" || presentInput.type == "�ؼ���do")
            {
                //D' -> S; D'
                print("[�﷨]�Ƶ���<��䲿��'> -> <���>;<��䲿��'> ");
                S();
                match("�ֺ�");
                Dp();
                return true;
            }
            if (presentInput.type == "�ؼ���end")
            {
                //D' -> ��
                print("[�﷨]�Ƶ���<��䲿��'> -> ��");
                return true;
            }
            return false;
        }
        private bool A()
        {
            //A -> i = E
            print("[�﷨]�Ƶ���<��ֵ���> -> ��ʶ�� = <���ʽ> ");
            string name = presentInput.value;
            match("��ʶ��");
            match("��ֵ��");
            identifier E1 = E();
            //E1�Ƿ�Ϊ��
            if (E1 == null)
            {
                print("��������ʶ�������븳ֵ���Ͳ�ƥ�䣬����ĺ������룡����");
                return false;
            }


            identifyTable.UpdateValue(name, E1.value);
            midTable.Add("=", E1.name, "null", name);
            print("[����]������Ԫʽ��" + name + " = " + E1.name);
            return true;
        }
        private bool C()
        {
            //C -> if (R) F else F
            print("[�﷨]�Ƶ���<�������> -> if ( <����> ) <Ƕ�����> else <Ƕ�����>");
            match("�ؼ���if");
            match("������");
            identifier t = R();

            int trueExit = midTable.NXQ+2;
            midTable.Add("jnz", t.name, "null", trueExit.ToString());
            int falseExit = midTable.NXQ;
            midTable.Add("j", "null", "null", "0");

            match("������");
            F();

            int Exit = midTable.NXQ;
            midTable.Add("j", "null", "null", "0");
            midTable.backpath(falseExit, midTable.NXQ.ToString());
            match("�ؼ���else");
            F();

            midTable.backpath(Exit, midTable.NXQ.ToString());
            return true;
        }
        private bool L()
        {
            //L -> do F while (R)
            print("[�﷨]�Ƶ���<ѭ�����> -> do <Ƕ�����> while ( <����> )");
            match("�ؼ���do");
            int begin = midTable.NXQ;
            F();
            match("�ؼ���while");
            match("������");
            identifier t = R();
            int trueExit = midTable.NXQ;
            midTable.Add("jnz", t.name, "null", begin.ToString());
            //int falseExit = midTable.NXQ;
            //midTable.Add("j", "null", "null", "0");
            //midTable.backpath(falseExit, midTable.NXQ.ToString());


            match("������");

            return true;
        }
        private bool F()
        {
            //F -> S | B
            if (presentInput.type == "�ؼ���start")
            {
                //F -> B
                print("[�﷨]�Ƶ���<Ƕ�����> -> <���>");
                B();
                return true;
            }
            if (presentInput.type == "��ʶ��" || presentInput.type == "�ؼ���if" || presentInput.type == "�ؼ���do")
            {
                //F -> S
                print("[�﷨]�Ƶ���<Ƕ�����> -> <�������> ");
                S();
                return true;
            }
            return false;
        }
        private bool B()
        {
            //B -> start D end
            print("[�﷨]�Ƶ���<�������> -> start <��䲿��> end");
            match("�ؼ���start");
            D();
            match("�ؼ���end");
            return true;
        }
        private identifier R()
        {
            //R -> E o E
            print("[�﷨]�Ƶ���<����> -> <���ʽ> ��ϵ����� <���ʽ>");
            identifier E1 = E();
            string operator1 = presentInput.value;
            identifier t = temptable.tempVar();
            //ƥ���ϵ�����
            //if (presentInput.type == "С��" || presentInput.type == "����" || presentInput.type == "С�ڵ���" || presentInput.type == "���ڵ���" || presentInput.type == "���ں�" || presentInput.type == "������")
            //{
            //    match(presentInput.type);
            //}
            if (presentInput.type == "С��")
            {
                match("С��");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //�Ƚ�E1��E2��value,�����򷵻�value = true
                if (e1.CompareTo(e2) < 0)
                {
                    t.value = "true";
                }
                else
                {
                    t.value = "false";
                }
                t.type = "bool";
                temptable.saveTempVar(t.name, t.type, t.value);

                midTable.Add(operator1, E1.name, E2.name, t.name);
                print("[����]������Ԫʽ��" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "����")
            {
                match("����");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //�Ƚ�E1��E2��value,�����򷵻�value = true
                if (e1.CompareTo(e2) > 0)
                {
                    t.value = "true";
                }
                else
                {
                    t.value = "false";
                }
                t.type = "bool";
                temptable.saveTempVar(t.name, t.type, t.value);
                midTable.Add(operator1, E1.name, E2.name, t.name);
                print("[����]������Ԫʽ��" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "С�ڵ���")
            {
                match("С�ڵ���");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //�Ƚ�E1��E2��value,�����򷵻�value = true
                if (e1.CompareTo(e2) <= 0)
                {
                    t.value = "true";
                }
                else
                {
                    t.value = "false";
                }
                t.type = "bool";
                temptable.saveTempVar(t.name, t.type, t.value);
                midTable.Add(operator1, E1.name, E2.name, t.name);
                print("[����]������Ԫʽ��" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "���ڵ���")
            {
                match("���ڵ���");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //�Ƚ�E1��E2��value,�����򷵻�value = true
                if (e1.CompareTo(e2) >= 0)
                {
                    t.value = "true";
                }
                else
                {
                    t.value = "false";
                }
                t.type = "bool";
                temptable.saveTempVar(t.name, t.type, t.value);
                midTable.Add(operator1, E1.name, E2.name, t.name);
                print("[����]������Ԫʽ��" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "���ں�")
            {
                match("���ں�");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //�Ƚ�E1��E2��value,�����򷵻�value = true
                if (e1.CompareTo(e2) == 0)
                {
                    t.value = "true";
                }
                else
                {
                    t.value = "false";
                }
                t.type = "bool";
                temptable.saveTempVar(t.name, t.type, t.value);
                midTable.Add(operator1, E1.name, E2.name, t.name);
                print("[����]������Ԫʽ��" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "������")
            {
                match("������");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //�Ƚ�E1��E2��value,�����򷵻�value = true
                if (e1.CompareTo(e2) != 0)
                {
                    t.value = "true";
                }
                else
                {
                    t.value = "false";
                }
                t.type = "bool";
                temptable.saveTempVar(t.name, t.type, t.value);
                midTable.Add(operator1, E1.name, E2.name, t.name);
                print("[����]������Ԫʽ��" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else
            {
                print("[�﷨]����ȱ�ٹ�ϵ�����");
            }
            //identifier E2 = E();
            //identifier t = temptable.tempVar();
            //t.type = "bool";
            //temptable.saveTempVar(t.name, t.type, t.value);

            //midTable.Add(operator1, E1.name, E2.name, t.name);


            //t.value


            return t;
        }

        private identifier E()
        {
            //E -> T E'
            print("[�﷨]�Ƶ���<���ʽ> -> <��> <���ʽ'>");
            identifier T1 = T();
            identifier Ep1 = Ep(T1);
            return Ep1;
        }
        private identifier Ep(identifier T1)
        {
            //E' -> + T E'| ��
            
            if (presentInput.type == "���Ӻ�")
            {
                print("[�﷨]�Ƶ���<���ʽ'> -> + <��> <���ʽ'>");
                //E' -> + T E'
                match("���Ӻ�");
                identifier T2 = T();
                if (T2 == null)
                {
                    print("[����]��������������Ͳ�һ�£������˳����򣡣���");
                    //��ֹ����
                    print("ϵͳ����10���ر�");
                    Thread.Sleep(10000);

                    System.Environment.Exit(0);
                }
                identifier t = temptable.tempVar();
                t.type = T1.type;
                t.value = T1.value + T2.value;
                midTable.Add("+", T1.name, T2.name, t.name);
                temptable.saveTempVar(t.name, t.type, t.value);
                print("[����]������Ԫʽ��" + t.name + " = " + T1.name + " + " + T2.name);

                identifier Ep2 = Ep(t);
                return Ep2;
            }
            if (presentInput.type == "�ֺ�" || presentInput.type == "������" || 
                presentInput.type == "С��" || presentInput.type == "����" || 
                presentInput.type == "С�ڵ���" || presentInput.type == "���ڵ���" || 
                presentInput.type == "���ں�" || presentInput.type == "������" ||
                presentInput.type == "�ؼ���while" || presentInput.type == "�ؼ���if" ||
                presentInput.type == "�ؼ���else" || presentInput.type == "�ؼ���end")
            {
                print("[�﷨]�Ƶ���<���ʽ'> -> ��");
                //E' -> ��
                return T1;
            }
            return null;
        }
        private identifier T()
        {
            //T -> G T'
            print("[�﷨]�Ƶ���<��> -> <����> <��'>");
            identifier G1 = G();
            identifier Tp1 = Tp(G1);
            return Tp1;
        }
        private identifier Tp(identifier G1)
        {
            //T' -> * n T' | ��

            if (presentInput.type == "�ظ������")
            {
                //T' -> * n T
                print("[�﷨]�Ƶ���<��'> -> * ���� <��'> ");
                match("�ظ������");
                if (presentInput.type =="��ʶ��")
                {
                    print("[����]��������������Ͳ�һ�£������˳����򣡣���");
                    //��ֹ����
                    print("ϵͳ����10���ر�");
                    Thread.Sleep(10000);
                    
                    System.Environment.Exit(0);
                }
                int n = int.Parse(presentInput.value);
                identifier n1 = new identifier("n");
                n1.type = "int";
                n1.value = "n";
                twoTuple num = consttable.newConst("int", n.ToString());
                string value = G1.value;
                identifier t = temptable.tempVar();
                t.type = G1.type;
                t.value = G1.value;
                for (int i = 1; i < n; i++)
                {
                    G1.value += value;
                    t.value = G1.value;
                }
                match("����");
                midTable.Add("*", G1.name, n.ToString(), t.name);
                temptable.saveTempVar(t.name, t.type, t.value);
                print("[����]������Ԫʽ��" + G1.name + " = " + G1.name + " * " + n.ToString());
                identifier Tp2 = Tp(t);
                return Tp2;
            }
            if (presentInput.type == "���Ӻ�" || presentInput.type == "�ֺ�" 
                || presentInput.type =="������" || presentInput.type == "������" 
                || presentInput.type == "�ؼ���while"|| presentInput.type =="����"
                || presentInput.type == "С��"||presentInput.type =="���ڵ���"
                || presentInput.type == "С�ڵ���" || presentInput.type == "���ں�"
                || presentInput.type =="�ؼ���else")
            {
                //T' ->  ��
                print("[�﷨]�Ƶ���<��'> ->��");
                return G1;
            }
            return null;
            
        }
        private identifier G()
        {
            //G -> i| s | (E)
            if (presentInput.type == "��ʶ��")
            {
                //G -> i
                print("[�﷨]�Ƶ���<����> -> ��ʶ�� ");
                string name = presentInput.value;
                match("��ʶ��");
                return identifyTable.getIdentifier(name);

            }
            if (presentInput.type == "�ַ���")
            {
                //G -> s
                print("[�﷨]�Ƶ���<����> -> �ַ��� ");
                string value = presentInput.value;
                match("�ַ���");
                identifier s = new identifier(value);
                s.value = value;
                s.type = "string";
                twoTuple str = consttable.newConst("string", s.value);
                return s;
            }
            if (presentInput.type == "������")
            {
                //G -> (E)
                print("[�﷨]�Ƶ���<����> -> �� <���ʽ> )");
                match("������");
                identifier E1 = E();
                match("������");
                return E1;
            }
            return null;
            
        }




        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            //��ȡin.txt�ļ�����
            StreamReader sr = new StreamReader("in.txt", Encoding.Default);
            //����ȡ�����ݸ�ֵ��textBox1
            textBox1.Text = sr.ReadToEnd();
            print("��ȡin.txt�ļ����ݳɹ����������£�");
            print(textBox1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            identifyTable = new identifierTable();
            midTable = new MiddleCodeTable();
            temptable = new tempVarTable();
            consttable = new constTable();

            inputProgram = textBox1.Text + "#";
            inputPointer = 0;
            presentInput = nextInput();
            print("������ַ���Ϊ��" );
            printInputProgram(inputProgram);
            P();
            //inputProgram = "string " + inputProgram;
            //print("�ʷ�����������£�");
            /*
            while (presentInput.type != "#")
            {
                presentInput = nextInput();
                print(presentInput.ToString());
            }
            print("�������������ʷ���������������������");*/
            print("��ʶ������Ԫʽ����ʱ���������£�");
            identifyTable.tablePrint(listBox1);
            //print("��������������ʶ�������������������");
            midTable.tablePrint(listBox1);
            temptable.tablePrint(listBox1);
            consttable.tablePrint(listBox1);
            //����ǰʶ����е�����д�뵽debug.txt�ļ���
            saveDebuglistBox1();
            print2("ʶ�����Ԫʽ���£�");
            midTable.tablePrint(listBox2);
            saveOutlistBox2();
            print2("�ѽ���Ԫʽ������out.txt�ļ���");
            print("�ѽ����ݱ�����debug.txt�ļ���");

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
