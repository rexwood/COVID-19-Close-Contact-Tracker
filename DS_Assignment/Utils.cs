using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;


public class Utils
{
    public static string[] places = {"北京师范大学", "人定湖公园", "西条胡同商业街","积水潭医院",
            "北京师范大学分校区","北京大学人民医院","北京动物园","枫蓝国际购物中心",}; //地点信息

    public static int personId = 0; //记录人员的id值
    public static int spreaderId = 0; //记录患者的id值 

    //功能：从txt文件读入Person数据，返回person动态数组:
    public static DynamicArray<Person> ReadTxt(string filePath)
    {
        
        StreamReader reader = new StreamReader(filePath, System.Text.Encoding.Default);
        string buff;
        DynamicArray<Person> vp = new DynamicArray<Person>();
        buff = reader.ReadLine();
        while(buff != null)
        {
            char[] chs = { ' ' };
            string[] items = buff.Split(chs, options: StringSplitOptions.RemoveEmptyEntries);
            string name = items[0];
            string dept = items[1];
            string addr = items[2];
            string tele = items[3];
            bool isProtected = Convert.ToBoolean(Convert.ToInt32(items[4]));
            bool isAffected = Convert.ToBoolean(Convert.ToInt32(items[5]));
            int numLoc = Convert.ToInt32(items[6]);
            int i = 0;
            DynamicArray<int> startTime = new DynamicArray<int>();
            DynamicArray<int> leftTime = new DynamicArray<int>();
            DynamicArray<int> locName = new DynamicArray<int>();

            for (i = 0; i < numLoc; i++)
            {
                startTime.add(Convert.ToInt32(items[7 + i]));
            }
            for (i = 0; i < numLoc; i++)
            {
                leftTime.add(Convert.ToInt32(items[7 + numLoc + i]));
            }
            for (i = 0; i < numLoc; i++)
            {
                locName.add(Convert.ToInt32(items[7 + numLoc * 2 + i]));
            }
            Person person = new Person(personId++, name, dept, tele, addr, isProtected, isAffected, false, numLoc, startTime, leftTime, locName);
            vp.add(person);
            buff = reader.ReadLine();
        }
        for(int i = 0; i < vp.count; i++)
        {
            vp.array[i].printInfo();
        }
        return vp;
    }

    //功能：将表示时间的int改为时间字符串（xx:xx）
    public static string ConvertInt2Time(int time)
    {
        int hour = time / 60;
        int min = time % 60;
        string strHour = hour.ToString().PadLeft(2, '0');
        string strMin = min.ToString().PadLeft(2, '0');
        return strHour + ":" + strMin;
    }

    //功能：将时间(xx:xx)改为表示时间的int
    public static int ConvertTime2Int(int hour, int min)
    {
        if (hour < 0 || hour > 23 || min < 0 || min > 59)
        {
            throw new Exception();
        }
        return hour * 60 + min;
    }

    //功能：将天数和时间转化成int
    public static int ConvertStringDay2Int(int day, string time)
    {
        string hour, min;
        char[] chs = { ':' };
        string[] items = time.Split(chs, options: StringSplitOptions.RemoveEmptyEntries);
        hour = items[0];
        min = items[1];
        int intTime = ConvertTime2Int(Convert.ToInt32(hour), Convert.ToInt32(min));
        return intTime + (day-1) * 1440;
        
    }

    //功能：将事件的string转换成int
    public static int ConvertString2Int(string time) //格式：xx:xx
    {
        string hour, min;
        char[] chs = { ':' };
        string[] items = time.Split(chs, options: StringSplitOptions.RemoveEmptyEntries);
        hour = items[0];
        min = items[1];
        return Utils.ConvertTime2Int(Convert.ToInt32(hour), Convert.ToInt32(min));

    }
    
    //（*）追踪算法：
    public static DynamicArray<Interval> Track(MainDS mds, Interval queryInterval, DynamicArray<Person> spreaders, out string log, DynamicArray<Person> personArray, int day)
    {
        log = "";
        DynamicArray<Interval> result = new DynamicArray<Interval>();//存放查询结果    //下一行再检查的时候一定要检查序号！！！！！！！！！！！！！！
        int loc = spreaders.array[queryInterval.getId()].findLoc(Convert.ToInt32(queryInterval.getStart()+1440*(day-1)), Convert.ToInt32(queryInterval.getEnd()+ 1440 * (day - 1)));
        //int loc = spreaders.array[queryInterval.getId()].findLoc(Convert.ToInt32(queryInterval.getStart()), Convert.ToInt32(queryInterval.getEnd()));
        Console.WriteLine("现在患者在：{0}", Utils.places[loc]);
        Console.WriteLine("时间为：DAY{0} {1}-{2}", day, Utils.ConvertInt2Time(Convert.ToInt32(queryInterval.getStart())), Utils.ConvertInt2Time(Convert.ToInt32(queryInterval.getEnd())));
        Console.WriteLine("密切接触者：");
        log += "现在患者"+ spreaders.array[queryInterval.getId()].name + "在：" + Utils.places[loc] + " 时间为：DAY"+day+" " + Utils.ConvertInt2Time(Convert.ToInt32(queryInterval.getStart())) + "~" + Utils.ConvertInt2Time(Convert.ToInt32(queryInterval.getEnd())) + " ";

        //查询：
        IntervalTree temp = mds.it[loc];
        temp.intersectInterval(temp.root, queryInterval, result, personArray);

        return result;
    }

    //功能：由Person转换而成的Interval动态数组
    public static DynamicArray<Interval> personToIntervals(Person p)
    {
        DynamicArray<Interval> queryIntervals = new DynamicArray<Interval>();
        int n = p.numLoc;
        for(int j = 0; j < n; j++)
        {
            queryIntervals.add(new Interval(p.startTime.array[j], p.leftTime.array[j], p.id));
        }

      return queryIntervals;
    }

    //功能：判断是否在person动态数组中已经记录了该person：
    public static bool IsPersonLogged(DynamicArray<Person> personArray, string personName)
    {
        for(int i = 0; i < personArray.count; i++)
        {
            if(personArray.array[i].name == personName)
            {
                return true;
            }
        }
        return false;
    }

    //功能：在相应数组中查找某一元素的下标
    public static int saveFindStringIndex(string s, string[] sArray)
    {
        int i = 0;
        for (i = 0; i < sArray.Length; i++)
        {
            if (places[i] == s)
            {
                break;
            }
        }
        if (i == sArray.Length) throw new Exception();
        return i;
    }

    //功能：创建cutTime数组，方便显示时间   //one person version!
    public static DynamicArray<Interval> createCutInterval(DynamicArray<Person> personArray)
    {
        DynamicArray<Interval> ret = new DynamicArray<Interval>();
        int i = 0;
        ret.add(new Interval(0, personArray.array[i].startTime.array[0], -1));
        for (i = 0; i < personArray.count-1; i++)
        {
            ret.add(new Interval(personArray.array[i].startTime.array[0], personArray.array[i].leftTime.array[0], - 1));
            ret.add(new Interval(personArray.array[i].leftTime.array[0], personArray.array[i + 1].startTime.array[0], -1));

        }
        ret.add(new Interval(personArray.array[i].leftTime.array[0], 1440, -1));
        return ret;
    } 

    //功能：按照区间的左端点对区间数组进行排序
    public static DynamicArray<Interval> sortByInterval(DynamicArray<Interval> intervals)
    {
        for(int i = 0; i < intervals.count-1; i++)
        {
            for(int j=0;j<intervals.count -1 - i; j++)
            {
                if (cmp(intervals.array[j], intervals.array[j + 1]) == true)
                {
                    Interval temp = intervals.array[j];
                    intervals.array[j] = intervals.array[j + 1];
                    intervals.array[j + 1] = temp;
                }
            }
        }
        /*        for(int i = 0; i < intervals.count; i++)
                {
                    Console.WriteLine(intervals.array[i].getStart() + ":" + intervals.array[i].getEnd());
                }*/
        return intervals;
    }

    //功能：比较函数（helper）
    public static bool cmp(Interval x, Interval y)
    {
        if (x.getStart() != y.getStart()) return x.getStart() >= y.getStart();
        else return x.getEnd() > y.getEnd();
    }

    //功能：记录密切接触者的信息：
    public static string reportContact(DynamicArray<Interval> result, string logmin, DynamicArray<Person> personArray)
    {
        if(result.count == 0)
        {
            logmin += "没有密接者";
            return logmin;
        }
        logmin += "密接者为：";
        for(int i = 0; i < result.count; i++)
        {
            string name = personArray.array[result.array[i].getId()].name;
            logmin += name;
            logmin += " ";
        }
        return logmin;
    }

    //功能：向文件里写入：
    public static void WriteFile(string fileName, string savePath, string content)
    {
        System.IO.StreamWriter sw = new System.IO.StreamWriter(savePath + "/"+ fileName+".txt", false, System.Text.Encoding.GetEncoding("gb2312"));
        sw.Write(content);
        sw.Flush();
        if (sw != null)
            sw.Close();
    }

    //功能：生成查询队列，时间从小到大排序，由spreaders生成：
    public static DynamicArray<Interval>[] generateQueryArray(DynamicArray<Person> spreaders)
    {
        /*DynamicArray<Interval> ret = new DynamicArray<Interval>();*/
        DynamicArray<Interval>[] ret = new DynamicArray<Interval>[3];
        for(int i = 0; i < 3; i++)
        {
            ret[i] = new DynamicArray<Interval>();
        }

        for (int i = 0; i < spreaders.count; i++)
        {
            for(int j = 0; j < spreaders.array[i].numLoc; j++)
            {
                ret[spreaders.array[i].startTime.array[j]/1440].add(new Interval(spreaders.array[i].startTime.array[j]%1440, spreaders.array[i].leftTime.array[j]%1440, spreaders.array[i].id));
            }
        }
        return ret;
    }

    //将queryArray3个变成一个，方便进行查询：
    public static DynamicArray<Interval> toOneArray(DynamicArray<Interval>[] queryArray)
    {
        DynamicArray<Interval> oneArray = new DynamicArray<Interval>();
        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < queryArray[i].count; j++)
            {
                oneArray.add(queryArray[i].array[j]);
            }
        }
        return oneArray;
    }
    
    //功能：计算式多少天
    public static int computeDay(int index, DynamicArray<Interval>[] queryArray)
    {
        if (index < queryArray[0].count)
        {
            return 0;
        }
        else if (index < queryArray[0].count + queryArray[1].count)
        {
            return 1;
        }
        else return 2;
    }
    
    //功能：找到上一个同ID的地址：
    public static int findLastLoc(int index, int id, DynamicArray<Interval> tempArray, Person p, int currentTime, int nowLoc)
    {
        int loc = -1;
        for(int i = index-1; i >= 0; i--)
        {
            if(id == tempArray.array[i].id)
            {
                for (int j = currentTime; j >= 0; j--)
                {
                    loc = p.findLoc(tempArray.array[i].getStart() + 1440 * j, tempArray.array[i].getEnd() + 1440 * j);
                    if (loc != nowLoc && loc != -1) return loc;
                }
            }
            
        }

        return loc;
    }
}