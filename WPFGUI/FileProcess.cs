using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
namespace WPFGUI
{
    public class FileProcess
    {
        public static List<Message> readFile(string inputFileName)
        {
            List<Message> msgs = new List<Message>();
            try
            {
                StreamReader sr = File.OpenText(inputFileName);
                string nextLine;
                nextLine = sr.ReadLine();
                while (true)
                {
                    if (nextLine == null) break;
                    string date = nextLine.Substring(0, nextLine.IndexOf(" "));
                    nextLine = nextLine.Substring(11, nextLine.Length - 11);
                    int space2 = nextLine.IndexOf(" ");
                    string time = nextLine.Substring(0, space2);
                    string[] date_time = { date, time };
                    string datetime_temp = String.Join(" ", date_time);
                    DateTime datetime = Convert.ToDateTime(datetime_temp);//提取日期、时间
                    string memberid_temp = nextLine.Substring(space2, nextLine.Length - space2);//提取群昵称和ID
                    int flag = memberid_temp.LastIndexOf("(");
                    if (memberid_temp.Contains("<"))
                    {
                        flag = memberid_temp.LastIndexOf("<");
                    }
                    string name = memberid_temp.Substring(1, flag - 1);
                    string member_type = "student";
                    if (name.Contains("助教"))
                    {
                        member_type = "assistant";
                    }
                    if (name.Contains("教师"))
                    {
                        member_type = "teacher";
                    }
                    if (name.Contains("系统消息"))
                    {
                        member_type = "system";
                    }
                    string id = memberid_temp.Substring(flag + 1, memberid_temp.Length - flag - 2);
                    //Console.WriteLine(datetime);
                    //Console.WriteLine(name);
                    //Console.WriteLine(member_type);
                    //Console.WriteLine(id);
                    //提取消息
                    string message = sr.ReadLine();
                    while (true)
                    {
                        string newline = sr.ReadLine();
                        if (newline == null)
                        {
                            nextLine = null;
                            break;
                        }
                        else
                        {
                            string pat = "\\d{4}-\\d{2}-\\d{2}.*";
                            Regex regex = new Regex(pat);
                            if (regex.IsMatch(newline))//读到新的一个人的发言
                            {
                                nextLine = newline;
                                break;
                            }
                            else//否则是这个人发言
                            {
                                message += newline;
                            }
                        }
                    }
                    string tag_pat = "#[^#]+#";
                    Regex tag_regex = new Regex(tag_pat);
                    MatchCollection tag_matchs = tag_regex.Matches(message);
                    List<string> theme = new List<string>();
                    foreach (Match tag in tag_matchs)
                    {
                        //Console.WriteLine(tag);
                        theme.Add(tag.Value);
                    }
                    message = Regex.Replace(message, tag_pat, "");
                    message = Regex.Replace(message, "、{2}", "");
                    //Console.WriteLine(message);
                    Message msg = new Message(datetime, member_type, name, id, message, theme);
                    msgs.Add(msg);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            //Console.WriteLine("filter done!");
            //Console.WriteLine(msgs.Count);
            /*
                for(int i = 0; i < msgs.Count; i++)
                {
                    Console.WriteLine(msgs[i].sendTime + " " + msgs[i].ID + " " + msgs[i].name + " " + msgs[i].message);
                    for (int j = 0; j < msgs[i].theme.Count; j++)
                    {
                        Console.Write(msgs[i].theme[j]+" ");
                    }
                    Console.WriteLine();
                }
            */
            return msgs;
        }
        public static Boolean writeFile(string outputFileName, WinnerGroup wg)
        {
            try
            {
                FileStream fs = new FileStream(outputFileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(wg.theme);
                for (int i = 0; i < wg.winnerGroup.Count; i++)
                {
                    if (i < wg.WinnerCondition.firstPrizeNumber)
                    {
                        sw.WriteLine("一等奖：" + wg.winnerGroup[i].name + "(" + wg.winnerGroup[i].ID + ")");
                    }
                    else if (i >= wg.WinnerCondition.firstPrizeNumber && i < wg.WinnerCondition.firstPrizeNumber + wg.WinnerCondition.secondPrizeNumber)
                    {
                        sw.WriteLine("二等奖：" + wg.winnerGroup[i].name + "(" + wg.winnerGroup[i].ID + ")");
                    }
                    else
                    {
                        sw.WriteLine("三等奖：" + wg.winnerGroup[i].name + "(" + wg.winnerGroup[i].ID + ")");
                    }
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }

    /*WinnerGroup   temp*/
    public class WinnerGroup
    {
        public string theme;
        public List<GroupMember> winnerGroup;
        public Condition WinnerCondition;
        public WinnerGroup(string themeKey, List<GroupMember> winners, Condition condition)
        {
            theme = themeKey;
            winnerGroup = winners;
            WinnerCondition = condition;
        }
    }

    public class Message
    {
        public DateTime sendTime;
        public string type;
        public string name;
        public string ID;
        public string message;
        public List<string> theme;
        public Message(DateTime stime, string type, string name, string id, string msg, List<string> theme)
        {
            this.sendTime = stime;
            this.type = type;
            this.name = name;
            this.ID = id;
            this.message = msg;
            this.theme = theme;
        }
    }
}