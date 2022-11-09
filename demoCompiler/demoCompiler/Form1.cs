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
        //将listBox1内容保存并输出到debug.txt中
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
        //将listBox2内容保存并输出到out.txt中
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
        //将输入串分行打印（遇分号转行，遇复合语句缩进）
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
        //10秒后进程终止
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
                lb.Items.Add("——————四元式表——————");
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
                lb.Items.Add("——————（标识符名，类型，值）——————");
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
                //判断类型与与值是否重复
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
                lb.Items.Add("——————（常量类型，对应值）——————");
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
                
                //保存临时变量
                table.Add(t.name);
                table.Add(t.type);
                table.Add(t.value);
                return t;
            }
            //保存type和value
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
                lb.Items.Add("——————临时变量表——————");
                //遍历打印所有临时变量和对应type和value,打印在一行内
                for (int i = 0; i < table.Count; i += 3)
                {
                    lb.Items.Add(" ( " + table[i] + ",  " + table[i + 1] + ",  " + table[i + 2] + "  )");
                }

                
            }
        }
        

        //词法分析函数
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
                        { tempWord += symbol; inputPointer++; return new twoTuple("分号", tempWord); }
                    }
                    if (symbol == ',')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("逗号", tempWord); }
                    }
                    if (symbol == '(')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("左括号", tempWord); }
                    }
                    if (symbol == ')')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("右括号", tempWord); }
                    }
                    if (symbol == '+')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("连接号", tempWord); }
                    }
                    if (symbol == '*')
                    {
                        { tempWord += symbol; inputPointer++; return new twoTuple("重复运算号", tempWord); }
                    }
                    if (symbol >= '0' && symbol <= '9') { state = 13; tempWord += symbol; inputPointer++; continue; }

                }
                if (state == 1201)
                {
                    if (symbol == '=')
                    {
                        tempWord += symbol; inputPointer++; return new twoTuple("等于号", tempWord);
                    }
                    return new twoTuple("赋值号", tempWord);
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
                          inputPointer++; return new twoTuple("字符串", tempWord); }
                    }
                }

                if (state == 0801)
                {
                    //<>
                    if (symbol == '>') { tempWord += symbol; inputPointer++; return new twoTuple("不等于", tempWord); }
                    //<
                    if (symbol == '=') { tempWord += symbol; inputPointer++; return new twoTuple("小于等于", tempWord); }
                    return new twoTuple("小于", tempWord);

                }
                if (state == 0901)
                {
                    //=
                    if (symbol == '=') { tempWord += symbol; inputPointer++; return new twoTuple("大于等于", tempWord); }
                    //
                    return new twoTuple("大于", tempWord);
                }
                if (state == 0401)
                {
                    if (symbol == 'f') { tempWord += symbol; inputPointer++; return new twoTuple("关键字if", tempWord); }
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
                    if (symbol == 'g') { tempWord += symbol; inputPointer++; return new twoTuple("关键字string", tempWord); }
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
                    if (symbol == 't') { tempWord += symbol; inputPointer++; return new twoTuple("关键字start", tempWord); }
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
                    if (symbol == 'e') { tempWord += symbol; inputPointer++; return new twoTuple("关键字else", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 0502)
                {
                    if (symbol == 'd') { tempWord += symbol; inputPointer++; return new twoTuple("关键字end", tempWord); }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        state = 1; tempWord += symbol; inputPointer++; continue;
                    }
                    state = 1; continue;
                }
                if (state == 1001)
                {
                    if (symbol == 'o') { tempWord += symbol; inputPointer++; return new twoTuple("关键字do", tempWord); }
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
                    if (symbol == 'e') { tempWord += symbol; inputPointer++; return new twoTuple("关键字while", tempWord); }
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
                    return new twoTuple("标识符", tempWord);
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
                    if (symbol == ' ') { tempWord += symbol; inputPointer++; return new twoTuple("数字", tempWord); }
                    return new twoTuple("数字", tempWord);
                }
                state = 0;
                tempWord = "";
                inputPointer++;
            }
            return new twoTuple("#", "#");


        }
        //匹配函数
        private bool match(string type)
        {
            if (type == presentInput.type)
            {
                print("[语法]：" + type + "与读头下" + presentInput.type + "相同，匹配成功");
                presentInput = nextInput();
                print("[词法]：识别到下一个单词为" + presentInput.ToString());
                return true;
            }
            print("[语法]：错误" + type + "与读头下" + presentInput.type + "不相同，匹配失败");
            //打印报错位置
            print("[词法]：错误位置为" + presentInput.ToString());
            //删除错误单词
            presentInput.type = "#";
            presentInput.value = "#";
            


            return false;
        }
        private bool P()
        {
            //P -> string I; D
            print("[语法]推导：<程序> -> string <标识符列表>; <语句部分>");
            match("关键字string");
            I("string");
            match("分号");
            D();
            return true;
        }
        private bool I(string type)
        {
            //I -> i I'
            print("[语法]推导：<标识符列表> -> 标识符,<标识符列表'>");
            string name = presentInput.value;
            match("标识符");
            identifyTable.UpdateType(name, type);
            Ip(type);
            return true;
        }
        private bool Ip(string type)
        {
            //I' -> ,i I' | ε
            if (presentInput.type == "逗号")
            {
                //I' -> ,i I' 
                print("[语法]推导：<标识符列表'> -> ,<标识符列表><标识符列表'>");
                match("逗号");
                string name = presentInput.value;
                match("标识符");
                identifyTable.UpdateType(name, type);
                Ip(type);
                return true;
            }
            if (presentInput.type == "分号")
            {
                //I' -> ε
                print("[语法]推导：<标识符列表'> -> ε");
                return true;
            }
            return false;
        }
        private bool D()
        {
            //D -> S; D'
            print("[语法]推导：<语句部分> -> <语句>;<语句部分'>");
            S();
            match("分号");
            Dp();
            return true;
        }
        private bool S()
        {
            //S -> A|C|L
            if (presentInput.type == "标识符")
            {
                print("[语法]推导：<语句> -> <赋值语句>");
                A();
                return true;
            }
            if (presentInput.type == "关键字if")
            {
                print("[语法]推导：<语句> -> <条件语句>");
                C();
                return true;
            }
            if (presentInput.type == "关键字do")
            {
                print("[语法]推导：<语句> -> <循环语句>");
                L();
                return true;
            }
            return false;
        }
        private bool Dp()
        {
            //D' -> S; D' | ε

            if (presentInput.type == "标识符" || presentInput.type == "关键字if" || presentInput.type == "关键字do")
            {
                //D' -> S; D'
                print("[语法]推导：<语句部分'> -> <语句>;<语句部分'> ");
                S();
                match("分号");
                Dp();
                return true;
            }
            if (presentInput.type == "关键字end")
            {
                //D' -> ε
                print("[语法]推导：<语句部分'> -> ε");
                return true;
            }
            return false;
        }
        private bool A()
        {
            //A -> i = E
            print("[语法]推导：<赋值语句> -> 标识符 = <表达式> ");
            string name = presentInput.value;
            match("标识符");
            match("赋值号");
            identifier E1 = E();
            //E1是否为空
            if (E1 == null)
            {
                print("！！！标识符类型与赋值类型不匹配，请更改后再输入！！！");
                return false;
            }


            identifyTable.UpdateValue(name, E1.value);
            midTable.Add("=", E1.name, "null", name);
            print("[语义]产生四元式：" + name + " = " + E1.name);
            return true;
        }
        private bool C()
        {
            //C -> if (R) F else F
            print("[语法]推导：<条件语句> -> if ( <条件> ) <嵌套语句> else <嵌套语句>");
            match("关键字if");
            match("左括号");
            identifier t = R();

            int trueExit = midTable.NXQ+2;
            midTable.Add("jnz", t.name, "null", trueExit.ToString());
            int falseExit = midTable.NXQ;
            midTable.Add("j", "null", "null", "0");

            match("右括号");
            F();

            int Exit = midTable.NXQ;
            midTable.Add("j", "null", "null", "0");
            midTable.backpath(falseExit, midTable.NXQ.ToString());
            match("关键字else");
            F();

            midTable.backpath(Exit, midTable.NXQ.ToString());
            return true;
        }
        private bool L()
        {
            //L -> do F while (R)
            print("[语法]推导：<循环语句> -> do <嵌套语句> while ( <条件> )");
            match("关键字do");
            int begin = midTable.NXQ;
            F();
            match("关键字while");
            match("左括号");
            identifier t = R();
            int trueExit = midTable.NXQ;
            midTable.Add("jnz", t.name, "null", begin.ToString());
            //int falseExit = midTable.NXQ;
            //midTable.Add("j", "null", "null", "0");
            //midTable.backpath(falseExit, midTable.NXQ.ToString());


            match("右括号");

            return true;
        }
        private bool F()
        {
            //F -> S | B
            if (presentInput.type == "关键字start")
            {
                //F -> B
                print("[语法]推导：<嵌套语句> -> <语句>");
                B();
                return true;
            }
            if (presentInput.type == "标识符" || presentInput.type == "关键字if" || presentInput.type == "关键字do")
            {
                //F -> S
                print("[语法]推导：<嵌套语句> -> <复合语句> ");
                S();
                return true;
            }
            return false;
        }
        private bool B()
        {
            //B -> start D end
            print("[语法]推导：<复合语句> -> start <语句部分> end");
            match("关键字start");
            D();
            match("关键字end");
            return true;
        }
        private identifier R()
        {
            //R -> E o E
            print("[语法]推导：<条件> -> <表达式> 关系运算符 <表达式>");
            identifier E1 = E();
            string operator1 = presentInput.value;
            identifier t = temptable.tempVar();
            //匹配关系运算符
            //if (presentInput.type == "小于" || presentInput.type == "大于" || presentInput.type == "小于等于" || presentInput.type == "大于等于" || presentInput.type == "等于号" || presentInput.type == "不等于")
            //{
            //    match(presentInput.type);
            //}
            if (presentInput.type == "小于")
            {
                match("小于");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //比较E1与E2的value,符合则返回value = true
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
                print("[语义]产生四元式：" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "大于")
            {
                match("大于");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //比较E1与E2的value,符合则返回value = true
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
                print("[语义]产生四元式：" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "小于等于")
            {
                match("小于等于");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //比较E1与E2的value,符合则返回value = true
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
                print("[语义]产生四元式：" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "大于等于")
            {
                match("大于等于");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //比较E1与E2的value,符合则返回value = true
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
                print("[语义]产生四元式：" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "等于号")
            {
                match("等于号");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //比较E1与E2的value,符合则返回value = true
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
                print("[语义]产生四元式：" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else if (presentInput.type == "不等于")
            {
                match("不等于");
                identifier E2 = E();
                //identifier t = temptable.tempVar();
                string e1 = E1.value;
                string e2 = E2.value;
                //比较E1与E2的value,符合则返回value = true
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
                print("[语义]产生四元式：" + t.name + " = " + E1.name + " " + operator1 + " " + E2.name);
            }
            else
            {
                print("[语法]错误：缺少关系运算符");
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
            print("[语法]推导：<表达式> -> <项> <表达式'>");
            identifier T1 = T();
            identifier Ep1 = Ep(T1);
            return Ep1;
        }
        private identifier Ep(identifier T1)
        {
            //E' -> + T E'| ε
            
            if (presentInput.type == "连接号")
            {
                print("[语法]推导：<表达式'> -> + <项> <表达式'>");
                //E' -> + T E'
                match("连接号");
                identifier T2 = T();
                if (T2 == null)
                {
                    print("[语义]错误：两个项的类型不一致，即将退出程序！！！");
                    //终止进程
                    print("系统将在10秒后关闭");
                    Thread.Sleep(10000);

                    System.Environment.Exit(0);
                }
                identifier t = temptable.tempVar();
                t.type = T1.type;
                t.value = T1.value + T2.value;
                midTable.Add("+", T1.name, T2.name, t.name);
                temptable.saveTempVar(t.name, t.type, t.value);
                print("[语义]产生四元式：" + t.name + " = " + T1.name + " + " + T2.name);

                identifier Ep2 = Ep(t);
                return Ep2;
            }
            if (presentInput.type == "分号" || presentInput.type == "右括号" || 
                presentInput.type == "小于" || presentInput.type == "大于" || 
                presentInput.type == "小于等于" || presentInput.type == "大于等于" || 
                presentInput.type == "等于号" || presentInput.type == "不等于" ||
                presentInput.type == "关键字while" || presentInput.type == "关键字if" ||
                presentInput.type == "关键字else" || presentInput.type == "关键字end")
            {
                print("[语法]推导：<表达式'> -> ε");
                //E' -> ε
                return T1;
            }
            return null;
        }
        private identifier T()
        {
            //T -> G T'
            print("[语法]推导：<项> -> <因子> <项'>");
            identifier G1 = G();
            identifier Tp1 = Tp(G1);
            return Tp1;
        }
        private identifier Tp(identifier G1)
        {
            //T' -> * n T' | ε

            if (presentInput.type == "重复运算号")
            {
                //T' -> * n T
                print("[语法]推导：<项'> -> * 数字 <项'> ");
                match("重复运算号");
                if (presentInput.type =="标识符")
                {
                    print("[语义]错误：两个项的类型不一致，即将退出程序！！！");
                    //终止进程
                    print("系统将在10秒后关闭");
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
                match("数字");
                midTable.Add("*", G1.name, n.ToString(), t.name);
                temptable.saveTempVar(t.name, t.type, t.value);
                print("[语义]产生四元式：" + G1.name + " = " + G1.name + " * " + n.ToString());
                identifier Tp2 = Tp(t);
                return Tp2;
            }
            if (presentInput.type == "连接号" || presentInput.type == "分号" 
                || presentInput.type =="不等于" || presentInput.type == "右括号" 
                || presentInput.type == "关键字while"|| presentInput.type =="大于"
                || presentInput.type == "小于"||presentInput.type =="大于等于"
                || presentInput.type == "小于等于" || presentInput.type == "等于号"
                || presentInput.type =="关键字else")
            {
                //T' ->  ε
                print("[语法]推导：<项'> ->ε");
                return G1;
            }
            return null;
            
        }
        private identifier G()
        {
            //G -> i| s | (E)
            if (presentInput.type == "标识符")
            {
                //G -> i
                print("[语法]推导：<因子> -> 标识符 ");
                string name = presentInput.value;
                match("标识符");
                return identifyTable.getIdentifier(name);

            }
            if (presentInput.type == "字符串")
            {
                //G -> s
                print("[语法]推导：<因子> -> 字符串 ");
                string value = presentInput.value;
                match("字符串");
                identifier s = new identifier(value);
                s.value = value;
                s.type = "string";
                twoTuple str = consttable.newConst("string", s.value);
                return s;
            }
            if (presentInput.type == "左括号")
            {
                //G -> (E)
                print("[语法]推导：<因子> -> （ <表达式> )");
                match("左括号");
                identifier E1 = E();
                match("右括号");
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
            //读取in.txt文件内容
            StreamReader sr = new StreamReader("in.txt", Encoding.Default);
            //将读取的内容赋值给textBox1
            textBox1.Text = sr.ReadToEnd();
            print("读取in.txt文件内容成功，内容如下：");
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
            print("输入的字符串为：" );
            printInputProgram(inputProgram);
            P();
            //inputProgram = "string " + inputProgram;
            //print("词法分析结果如下：");
            /*
            while (presentInput.type != "#")
            {
                presentInput = nextInput();
                print(presentInput.ToString());
            }
            print("——————词法分析结束——————");*/
            print("标识符表、四元式表、临时变量表如下：");
            identifyTable.tablePrint(listBox1);
            //print("——————标识符表结束——————");
            midTable.tablePrint(listBox1);
            temptable.tablePrint(listBox1);
            consttable.tablePrint(listBox1);
            //将当前识别表中的内容写入到debug.txt文件中
            saveDebuglistBox1();
            print2("识别的四元式如下：");
            midTable.tablePrint(listBox2);
            saveOutlistBox2();
            print2("已将四元式保存至out.txt文件中");
            print("已将内容保存至debug.txt文件中");

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
