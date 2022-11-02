using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//人物信息（待检测者）
public class Person
{
    public int id;           //序号（键码）
    public string name;
    public string dept;
    public string addr;
    public string tele;
    public bool isProtected; //是否有保护措施
    public bool isAffected;  //是否在无保护措施的前提下被患者接触
    public bool isSpreader;  //是否是患者
    public int numLoc;       //到过地点的个数
    public DynamicArray<int> startTime; //存储到达时间的数组，长度为numLoc
    public DynamicArray<int> leftTime; //存储离开时间的数组，长度为numLoc
    public DynamicArray<int> locName;  //存储地点的数组，长度为numLoc


    //构造函数：
    public Person(int id, string name, string dept, string addr,
        string tele, bool isProtected, bool isAffected, bool isSpreader,
        int numLoc, DynamicArray<int> startTime, DynamicArray<int> leftTime, DynamicArray<int> locName)
    {
        this.id = id;
        this.name = name;
        this.dept = dept;
        this.tele = tele;
        this.addr = addr;
        this.isProtected = isProtected;
        this.isAffected = isAffected;
        this.numLoc = numLoc;
        this.isSpreader = isSpreader;
        this.startTime = startTime;
        this.leftTime = leftTime;
        this.locName = locName;
    }

    //功能：打印人物信息：
    public void printInfo()
    {
        Console.WriteLine("ID: {0}", this.id);
        Console.WriteLine("Name:{0}", this.name);
        Console.WriteLine();
    }

    //功能：给定start和end 查找该人当前出现的地点
    public int findLoc(int start, int left)
    {
        for(int i=0; i < this.numLoc; i++)
        {
            if(this.startTime.array[i] == start && this.leftTime.array[i] == left)
            {
                return this.locName.array[i];
            }
        }
        return -1;
    }

    /*//功能：判断如果基本信息一致的话，两人是一个人，将他去过的地点合并（再添加人员/患者信息的时候使用）
    public bool merge(Person p2)
    {
        if(this.name == p2.name && this.tele == p2.tele)//是一个人
        {
            for(int i = 0; i < p2.numLoc; i++)
            {
                this.startTime.add(p2.startTime.array[i]);
                this.leftTime.add(p2.leftTime.array[i]);
                this.locName.add(p2.locName.array[i]);
            }
        }
        return false;
    }*/
}