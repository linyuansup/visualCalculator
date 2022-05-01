using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

/*
 * TODO：
*/
/*
 * DONE：
 * 输入内容检测
 * 精度报告
 * 界面
 * 基本计算功能
 * 逻辑运算符
 * 负号处理（需要括号）
 * × ÷ 支持
 * 错误处理：输入错误、除0、不知道什么错误的错误
 * 任意位置输入内容
 * 每次计算后的数值重新利用
 * enter 计算
 * backspace 删除
*/

namespace Visual_Calculator
{
    public partial class MainWindow : Window
    {
        public bool CalculateFinish = false;
        public int LeftBranck = 0;

        private void DeleteData()
        {
            if (TextData.Text == "")
            {
                SendInformation("表达式已清空");
                return;
            }
            if (TextData.Text.EndsWith("&") || TextData.Text.EndsWith("|"))
                TextData.Text = TextData.Text.Substring(0, TextData.Text.Length - 1);
            else if (TextData.Text.EndsWith("("))
                LeftBranck--;
            else if (TextData.Text.EndsWith(")"))
                LeftBranck++;
            TextData.Text = TextData.Text.Substring(0, TextData.Text.Length - 1);
        }

        private void InsertData(char x)
        {
            if (x == '！')
            {
                SendInformation("尽量避免在中文输入法模式下输入内容");
                x = '!';
            }
            if (x == '*')
                x = '×';
            else if (x == '/')
                x = '÷';
            string str = TextData.Text;
            if ((str == "" || str == "-") && (x == '+' || x == '*' || x == '/' || x == '×' || x == '÷' || x == 'A' || x == 'O'))
            {
                string temp = x.ToString();
                if (temp == "A")
                    temp = "&&";
                else if (temp == "O")
                    temp = "||";
                SendInformation("表达式为空时不允许使用运算符 " + temp);
                return;
            }
            else if ((x == '+' || x == '-' || x == '×' || x == '÷' || x == 'A' || x == 'O') && str != "" && (str[str.Length - 1] == '+' || str[str.Length - 1] == '-' || str[str.Length - 1] == '×' || str[str.Length - 1] == '÷' || str[str.Length - 1] == '&' || str[str.Length - 1] == '|' || str[str.Length - 1] == '!'))
            {
                if (x == '-')
                    SendInformation("表示负数时，请使用括号");
                TextData.Text = TextData.Text.Substring(0, TextData.Text.Length - 1);
                if (str != "" && (str[str.Length - 1] == '|' || str[str.Length - 1] == '&'))
                    TextData.Text = TextData.Text.Substring(0, TextData.Text.Length - 1);
            }
            else if (x == '!' && str != "" && str[str.Length - 1] >= '0' && str[str.Length - 1] <= '9')
            {
                SendInformation("不允许在数字后使用非运算符");
                return;
            }
            else if (x == '(')
                LeftBranck++;
            else if (x == ')')
            {
                if (LeftBranck == 0)
                {
                    SendInformation("没有足够的左括号与右括号匹配");
                    return;
                }
                LeftBranck--;
            }
            else if ((x == 'A' || x == 'O') && str[str.Length - 1] == '!')
            {
                SendInformation("不允许将非更改为逻辑运算符");
                return;
            }
            if (x >= '0' && x <= '9' && CalculateFinish)
            {
                ClearAll();
                CalculateFinish = false;
            }
            string add = x.ToString();
            if (add == "A")
                add = "&&";
            if (add == "O")
                add = "||";
            CalculateFinish = false;
            TextData.Text += add;
        }

        private void BP_Click(object sender, RoutedEventArgs e)
        {
            DeleteData();
        }

        private void ClearAll()
        {
            TextData.Text = "";
            HistoryData.Content = "";
            CalculateFinish = false;
            LeftBranck = 0;
        }

        private void Keyboard_in(object sender, TextCompositionEventArgs e)
        {
            if ((e.Text[0] >= '0' && e.Text[0] <= '9') || e.Text[0] == '+' || e.Text[0] == '-' || e.Text[0] == '!' || e.Text[0] == '！' || e.Text[0] == '(' || e.Text[0] == ')')
                InsertData(e.Text[0]);
            else if (e.Text[0] == '|')
                InsertData('O');
            else if (e.Text[0] == '|')
                InsertData('A');
            else if (e.Text[0] == '*')
                InsertData('×');
            else if (e.Text[0] == '/')
                InsertData('÷');
            else if (e.Text[0] == '=')
                Calculating();
        }

        private void SendInformation(string str)
        {
            InformationBar.MessageQueue?.Enqueue(str, null, null, null, false, true, TimeSpan.FromSeconds(2));
        }

        private void Calculating()
        {
            if (LeftBranck != 0)
                throw new Exception("括号匹配错误");
            if (TextData.Text == "")
                throw new Exception("表达式为空");
            string str = "(" + TextData.Text + ")=";
            HistoryData.Content = TextData.Text + "=";
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '|')
                    if (str[i + 1] == '|')
                    {
                        str = str.Remove(i, 2);
                        str = str.Insert(i, "O");
                        i++;
                    }
                    else
                        throw new ArgumentException("错误的双目表达式");
                else if (str[i] == '&')
                    if (str[i + 1] == '&')
                    {
                        str = str.Remove(i, 2);
                        str = str.Insert(i, "A");
                        i++;
                    }
                    else
                        throw new ArgumentException("错误的双目表达式");
            }
            for (int i = 0; i < str.Length; i++)
                if (str[i] == '(' && str[i + 1] == '-')
                    str = str.Insert(++i, "0");
            Stack<int> NumStack = new Stack<int>();
            Stack<char> OpeStack = new Stack<char>();
            bool DoubleBrac = false;
            for (int i = 0; ; i++)
            {
                if (str[i] >= '0' && str[i] <= '9')
                {
                    int temp = 0;
                    for (; i < str.Length; i++)
                    {
                        if (str[i] < '0' || str[i] > '9')
                        {
                            i--;
                            NumStack.Push(temp);
                            break;
                        }
                        temp = temp * 10 + str[i] - '0';
                    }
                }
                else if (str[i] == '+' || str[i] == '-' || str[i] == '×' || str[i] == '÷' || str[i] == '(' || str[i] == ')' || str[i] == '=' || str[i] == 'A' || str[i] == 'O' || str[i] == '!')
                {
                    if (str[i] == '(')
                    {
                        OpeStack.Push(str[i]);
                        continue;
                    }
                    if (OpeStack.Count != 0)
                    {
                        char LeftOpe;
                        char RightOpe;
                        do
                        {
                            LeftOpe = OpeStack.Peek();
                            RightOpe = str[i];
                            if (LeftOpe == '(' && RightOpe == ')')
                            {
                                DoubleBrac = true;
                                break;
                            }
                            else if (LeftOpe == RightOpe && LeftOpe == '!')
                                break;
                            else if (Priority(LeftOpe) >= Priority(RightOpe))
                            {
                                OpeStack.Pop();
                                int RightNum = NumStack.Pop();
                                if (LeftOpe != '!')
                                {
                                    int LeftNum = NumStack.Pop();
                                    NumStack.Push(GetAns(LeftNum, RightNum, LeftOpe));
                                }
                                else
                                    if (RightNum == 0)
                                    NumStack.Push(1);
                                else
                                    NumStack.Push(0);
                            }
                        }
                        while (Priority(LeftOpe) >= Priority(RightOpe) && OpeStack.Count != 0);
                    }
                    if (str[i] == '=')
                    {
                        TextData.Text = NumStack.Peek().ToString();
                        CalculateFinish = true;
                        break;
                    }
                    if (DoubleBrac)
                    {
                        OpeStack.Pop();
                        DoubleBrac = false;
                    }
                    else
                        OpeStack.Push(str[i]);
                }
                else
                    throw new Exception("表达式非法且未预定义");
            }
        }

        public int GetAns(int a, int b, char c)
        {
            switch (c)
            {
                case '+':
                    return a + b;
                case '-':
                    return a - b;
                case '×':
                    return a * b;
                case '÷':
                    if (b != 0)
                    {
                        if (a % b != 0)
                            SendInformation("本次计算可能存在精度误差");
                        return a / b;
                    }
                    throw new ArgumentException("除数为 0");
                case 'A':
                    if (a != 0 && b != 0)
                        return 1;
                    return 0;
                case 'O':
                    if (a == 0 && b == 0)
                        return 0;
                    return 1;
                case '!':
                    if (a == 0)
                        return 1;
                    return 0;
                default:
                    throw new ArgumentException("表达式存在运算符错误");
            }
        }

        public int Priority(char x)
        {
            switch (x)
            {
                case '=':
                    return 1;
                case ')':
                case '(':
                    return 2;
                case 'O':
                    return 3;
                case 'A':
                    return 4;
                case '+':
                case '-':
                    return 5;
                case '÷':
                case '×':
                    return 6;
                case '!':
                    return 7;
                default:
                    throw new ArgumentException("表达式存在运算符错误");
            }
        }

        private void CE_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void Num0_Click(object sender, RoutedEventArgs e)
        {
            InsertData('0');
        }

        private void Num1_Click(object sender, RoutedEventArgs e)
        {
            InsertData('1');
        }

        private void Num2_Click(object sender, RoutedEventArgs e)
        {
            InsertData('2');
        }

        private void Num3_Click(object sender, RoutedEventArgs e)
        {
            InsertData('3');
        }

        private void Num4_Click(object sender, RoutedEventArgs e)
        {
            InsertData('4');
        }

        private void Num5_Click(object sender, RoutedEventArgs e)
        {
            InsertData('5');
        }

        private void Num6_Click(object sender, RoutedEventArgs e)
        {
            InsertData('6');
        }

        private void Num7_Click(object sender, RoutedEventArgs e)
        {
            InsertData('7');
        }

        private void Num8_Click(object sender, RoutedEventArgs e)
        {
            InsertData('8');
        }

        private void Num9_Click(object sender, RoutedEventArgs e)
        {
            InsertData('9');
        }

        private void LeftBrac_Click(object sender, RoutedEventArgs e)
        {
            InsertData('(');
        }

        private void RightBrac_Click(object sender, RoutedEventArgs e)
        {
            InsertData(')');
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            InsertData('+');
        }

        private void Sub_Click(object sender, RoutedEventArgs e)
        {
            InsertData('-');
        }

        private void Multi_Click(object sender, RoutedEventArgs e)
        {
            InsertData('×');
        }

        private void Divide_Click(object sender, RoutedEventArgs e)
        {
            InsertData('÷');
        }

        private void And_Click(object sender, RoutedEventArgs e)
        {
            InsertData('A');
        }

        private void Or_Click(object sender, RoutedEventArgs e)
        {
            InsertData('O');
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            InsertData('!');
        }

        private void Equal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Calculating();
            }
            catch (Exception ex)
            {
                HistoryData.Content = "Error: " + ex.Message;
            }
        }

        private void KeyCalculating(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Calculating();
            else if (e.Key == Key.Back)
                DeleteData();
        }
    }
}
