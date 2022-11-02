using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public enum COLOR { RED, BLACK };
public class Interval //红黑区间树的结点
{

    public int start;
    public int end;
    public int max;
    public int id;  //人员的id信息
    public Interval left;
    public Interval right;
    public Interval parent;
    public COLOR color;
    
    //构造函数
    public Interval(int start, int end, int id)
    {
        this.start = start;
        this.end = end;
        this.max = end;
        this.id = id;
        this.color = COLOR.RED;
    }

    //返回uncle
    public Interval uncle()
    {
        if (this.parent == null || this.parent.parent == null)
            return null;

        if (this.parent.isOnLeft()) //uncle on right
            return this.parent.parent.right;

        else
            return this.parent.parent.left;
    }

    //检查该节点是不是父节点的左子
    public bool isOnLeft()
    {
        return this == parent.left;
    }

    //返回兄弟结点
    public Interval sibling()
    {
        if (this.parent == null)
            return null;

        if (isOnLeft())
            return parent.right;

        return parent.left;
    }

    //结点下移并再该位置上换上新节点
    public void moveDown(Interval nParent)
    {
        if (this.parent != null)
        {
            if (isOnLeft())
            {
                parent.left = nParent;
            }
            else
            {
                parent.right = nParent;
            }
        }
        nParent.parent = parent;
        parent = nParent;
    }
    
    //判断是否有红色孩子结点
    public bool hasRedChild()
    {
        return (left != null && left.color == COLOR.RED) ||
            (right != null && right.color == COLOR.RED);
    }

    public int getStart()
    {
        return this.start;
    }

    public int getEnd()
    {
        return this.end;
    }

    public int getMax()
    {
        return this.max;
    }

    public Interval getLeft()
    {
        return this.left;
    }

    public Interval getRight()
    {
        return this.right;
    }

    public int getId()
    {
        return this.id;
    }

    public void setMax(int max)
    {
        this.max = max;
    }

    public void setLeft(Interval left)
    {
        this.left = left;
    }

    public void setRight(Interval right)
    {
        this.right = right;
    }

    public void setId(int id)
    {
        this.id = id;
    }
    
    //打印结点信息：
    public override string ToString()
    {
        return "[" + this.start + "," + this.end + "," + this.max + "] ";
    }
    //用于比较结点的大小（为了排序和建立BST）
    public int compareTo(Interval i)
    {
        if (this.start < i.start)
        {
            return -1;
        }
        else if (this.start == i.start)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}

public class IntervalTree
{
    public Interval root;

    
    //功能：打印数（中序遍历）
    public static void printTree(Interval tmp)
    {
        if (tmp == null)
        {
            return;
        }

        if (tmp.getLeft() != null)
        {
            printTree(tmp.getLeft());
        }

        Console.Write(tmp);

        if (tmp.getRight() != null)
        {
            printTree(tmp.getRight());
        }
    }

    //功能：按照区间树查询区间重合
    public void intersectInterval(Interval tmp, Interval i, DynamicArray<Interval> res, DynamicArray<Person> personArray)
    {
        //说明：tmp为当前结点，i为待查询结点
        if (tmp == null)
        {
            return;
        }
        //查询看是否和当前结点有重合的区间(*)
        if ((!((tmp.getStart() > i.getEnd()) || (tmp.getEnd() < i.getStart())) )&& personArray.array[tmp.getId()].isProtected == false)
        {

            if (res == null)
            {
                res = new DynamicArray<Interval>();
            }
            res.add(tmp);
        }
        //剪枝
        if ((tmp.getLeft() != null) && (tmp.getLeft().getMax() >= i.getStart()))
        {
            this.intersectInterval(tmp.getLeft(), i, res, personArray);
        }

        this.intersectInterval(tmp.getRight(), i, res, personArray);
    }

    //功能：插入结点（点）
    public void stabInterval(Interval tmp, int i, DynamicArray<Interval> res)
    {
        //tmp为当前结点，i为待查时间点
        if(tmp == null)
        {
            return;
        }
        //查询看i是否在当前Interval内
        if (!((tmp.getStart() > i) || (tmp.getEnd() < i)))
        {
            if (res == null)
            {
                res = new DynamicArray<Interval>();
            }
            res.add(tmp);
        }

        if ((tmp.getLeft() != null) && (tmp.getLeft().getMax() >= i))
        {
            this.stabInterval(tmp.getLeft(), i, res);
        }
        this.stabInterval(tmp.getRight(), i, res);
    }

    //左旋
    void leftRotate(Interval x)
    {
        Interval nParent = x.right;

        //update root id current node is root
        if (x == root)
            root = nParent;

        x.moveDown(nParent);
        //connect x with new parent's left element
        x.right = nParent.left;
        //connect new arent's left element with node
        //if it is not null
        if (nParent.left != null)
            nParent.left.parent = x;

        //connect new parent with x
        nParent.left = x;
    }
    
    //右旋
    void rightRotate(Interval x)
    {
        // new parent will be node's left child 
        Interval nParent = x.left;

        // update root if current node is root 
        if (x == root)
            root = nParent;

        x.moveDown(nParent);

        // connect x with new parent's right element 
        x.left = nParent.right;
        // connect new parent's right element with node 
        // if it is not null 
        if (nParent.right != null)
            nParent.right.parent = x;

        // connect new parent with x 
        nParent.right = x;
    }

    //交换颜色
    void swapColors(Interval x1, Interval x2)
    {
        COLOR temp;
        temp = x1.color;
        x1.color = x2.color;
        x2.color = temp;
    }

    //解决红红冲突
    void fixRedRed(Interval x)
    {
        //if x is root color it black and return
        if (x == root)
        {
            x.color = COLOR.BLACK;
            return;
        }

        //initialize parent, grandparent, uncle
        Interval parent = x.parent, grandparent = parent.parent,
              uncle = x.uncle();

        if (parent.color != COLOR.BLACK)
        {
            if (uncle != null && uncle.color == COLOR.RED)
            {
                // uncle red, perform recoloring and recurse 
                parent.color = COLOR.BLACK;
                uncle.color = COLOR.BLACK;
                grandparent.color = COLOR.RED;
                fixRedRed(grandparent);
            }
            else
            {
                // Else perform LR, LL, RL, RR 
                if (parent.isOnLeft())
                {
                    if (x.isOnLeft())
                    {
                        // for left right 
                        swapColors(parent, grandparent);
                    }
                    else
                    {
                        leftRotate(parent);
                        swapColors(x, grandparent);
                    }
                    // for left left and left right 
                    rightRotate(grandparent);
                }
                else
                {
                    if (x.isOnLeft())
                    {
                        // for right left 
                        rightRotate(parent);
                        swapColors(x, grandparent);
                    }
                    else
                    {
                        swapColors(parent, grandparent);
                    }

                    // for right right and right left 
                    leftRotate(grandparent);
                }
            }
        }
    }

    //红黑树查找
    public Interval search(Interval interval)
    {
        Interval temp = root;
        while (temp != null)
        {
            if (interval.start < temp.start || (interval.start == temp.start && interval.end < temp.end))
            {
                if (temp.left == null)
                    break;
                else
                    temp = temp.left;
            }
            else if (interval.start == temp.start && interval.end == temp.end)
            {
                break;
            }
            else
            {
                if (temp.right == null)
                    break;
                else
                    temp = temp.right;
            }
        }

        return temp;
    }

    // 插入结点
    public void insert(Interval newNode)
    {
        if (root == null)
        {
            // when root is null 
            // simply insert value at root 
            newNode.color = COLOR.BLACK;
            root = newNode;
        }

        else
        {
            Interval temp = search(newNode);

            if (temp == newNode)
            {
                // return if value already exists 
                return;
            }

            // if value is not found, search returns the node 
            // where the value is to be inserted 

            // connect new node to correct node 
            newNode.parent = temp;

            if (newNode.start < temp.start)
                temp.left = newNode;
            else
                temp.right = newNode;

            // fix red red voilaton if exists 
            fixRedRed(newNode);
            //update max:
            Interval z = newNode;
            while (z != null)
            {
                if (z.left == null && z.right != null)
                    z.max = Math.Max(z.right.max, z.end);
                else if (z.right == null && z.left != null)
                    z.max = Math.Max(z.left.max, z.end);
                else if (z.right == null && z.left == null)
                    z.max = z.end;
                else
                    z.max = Math.Max(Math.Max(z.end, z.left.max), z.right.max);
                z = z.parent;
            }
        }


    }
}
