using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MainDS
{
    public IntervalTree[] it;
    //线性存储每个地点的相应编号的人的信息StartTime，leftTime，id

    //构造函数
    public MainDS()
    {
        it = new IntervalTree[8];
        for(int i=0; i < 8; i++)
        {
            it[i] = new IntervalTree();
        }
    }

    //功能：将personArray中的数据写入IntervalTree（*）
    public void fromDynamicArray2Tree(DynamicArray<Person> personArray)
    {
        for(int i = 0; i < personArray.count; i++)
        {
            if (personArray.array[i] != null)
                addPerson(personArray.array[i]);
        }
    }

    //功能：将一个任务的信息输入数据结构
    public void addPerson(Person p)
    {
        int n = p.numLoc;
        for(int i = 0; i < n; i++)
        {
            //it[p.locName.array[i]].root = IntervalTree.insertNode(it[p.locName.array[i]].root, new Interval(p.startTime.array[i], p.leftTime.array[i], p.id));
            it[p.locName.array[i]].insert(new Interval(p.startTime.array[i], p.leftTime.array[i], p.id));
        }
    }
}