using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Dynamic Array:
public class DynamicArray<T>
{
    public T[] array;
    public int count;
    public int size;

    //构造函数:
    public DynamicArray()
    {
        array = new T[1];
        count = 0;
        size = 1;
    }

    //功能:在array末尾添加一个元素
    public void add(T data)
    {
        //检测是否需要扩容
        if(count == size)
        {
            growSize(); //扩容函数
        }

        //在array末尾添加元素
        array[count] = data;
        count++;
    }

    //功能：扩充数组容量，大小变为原来的2倍
    public void growSize()
    {
        T[] temp = null;
        if(count == size)
        {
            //temp是临时数组，大小为原数组的两倍
            temp = new T[size * 2];

            for(int i = 0; i < size; i++)
            {
                //将原数组中的数组赋值到新数组中
                temp[i] = array[i];
            }
        }

        array = temp;
        size = size * 2;
    }

    //功能：在示当情况下（count < size/2）时，对数组长度进行缩减
    public void shrinkSize()
    {
        T[] temp = null;
        if(count > 0)
        {
            //temp的长度为count
            temp = new T[count];
            for(int i = 0; i < count; i++)
            {
                //将原数组中的元素赋值到新数组中
                temp[i] = array[i];
            }

            size = count;

            array = temp;
        }
    }

    //功能：在相应的下标加入元素
    public void addAt(int index, T data)
    {
        //如果满，扩容
        if(count == size)
        {
            growSize();
        }

        //移动插入下标邮编的元素
        for(int i=count - 1; i >= index; i--)
        {
            array[i + 1] = array[i];
        }

        //插入元素
        array[index] = data;
        count++;
    }

    //功能：移除最后一个元素在末尾补充default(null)
    public void remove()
    {
        if(count > 0)
        {
            array[count - 1] = default(T);
            count--;
        }
    }

    //功能：移除相应下标的元素
    public void removeAt(int index)
    {
        if(count > 0)
        {
            for(int i=index; i < count - 1; i++)
            {
                //index后面的元素全部左移一位
                array[i] = array[i + 1];
            }
            array[count - 1] = default(T);
            count--;
        }
    }
}
