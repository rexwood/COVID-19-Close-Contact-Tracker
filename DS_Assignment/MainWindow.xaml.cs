using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.OpenFileDialog;
using System.Runtime.InteropServices;
using System.Windows.Media.TextFormatting;
using System.Windows;




namespace DS_Assignment
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //变量：
        int count = 0; //查询次数
        DynamicArray<Person> spreaders = new DynamicArray<Person>(); //存储的患者们
        DynamicArray<Person> personArray = new DynamicArray<Person>(); //存储的人员们
        //DynamicArray<Interval> queryArray = new DynamicArray<Interval>(); //查询队列，时间从小到大排序，由spreaders生成
        DynamicArray<Interval>[] queryArray = new DynamicArray<Interval>[3];
        DynamicArray<Interval> tempArray = new DynamicArray<Interval>(); //存放真正的queryArray

        string openPath; //打开文件的路径
        string savePath; //保存文件的路径
        string log = ""; //记录查询的信息
        MainDS mds = new MainDS(); //主数据结构
        int currentTime = 0; //当前是第几天

        //主程序：
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "密接追踪器";
            for(int i = 0; i < 3; i++)
            {
                queryArray[i] = new DynamicArray<Interval>();
            }
        }

        /*
            上控制面板应用的函数
         */ 

        //选择人员信息的文件：
        private void btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Title = "C# Corner Open File Dialog";
            openFileDialog1.InitialDirectory = @"C:\Users\Lenovo\Desktop";
            openFileDialog1.Filter = "All files(*.*)|*.* | All files（*.*）| *.*";
            /*
             * FilterIndex 属性用于选择了何种文件类型,缺省设置为0,系统取Filter属性设置第一项
             * ,相当于FilterIndex 属性设置为1.如果你编了3个文件类型，当FilterIndex ＝2时是指第2个.
             */
            openFileDialog1.FilterIndex = 2;
            /*
             *如果值为false，那么下一次选择文件的初始目录是上一次你选择的那个目录，
             *不固定；如果值为true，每次打开这个对话框初始目录不随你的选择而改变，是固定的  
             */
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                openPath = System.IO.Path.GetFullPath(openFileDialog1.FileName); //绝对路径
                Console.WriteLine(openPath);
                this.openFile.Text = openPath;
               
            }
            this.btnLoad.IsEnabled = true;
            this.CurrentState.Content = "就绪";

        }

        //功能：选择保存的位置：
        private void btn_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.Description = "打开保存的路径";
            
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                savePath = System.IO.Path.GetFullPath(folderBrowser.SelectedPath).ToString();
                this.saveFile.Text = savePath;
            }
        }

        //将openPath对应的文件（人员信息）导入personArray
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DynamicArray<Person> temp = Utils.ReadTxt(openPath);
                for (int i = 0; i < temp.count; i++)
                {
                    personArray.add(temp.array[i]);
                }
                mds = new MainDS();
                mds.fromDynamicArray2Tree(personArray);
                this.btnLoad.IsEnabled = false;
                updateSubpages();
                MessageBox.Show("成功打开文件！");
            }
            catch
            {
                MessageBox.Show("打开文件失败！");
            }
        }

        //查询按钮的点击事件
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            int sumCount = queryArray[0].count + queryArray[1].count + queryArray[2].count;
            tempArray = Utils.toOneArray(queryArray);
            //Utils.TrackCOVID(mds, personArray, spreaders);
            //利用多线程实现程序的暂停和继续运行?--（不是）
            count = 0;
            if (count < tempArray.count)
            {
                string logmin = "";
                //DynamicArray<Interval> result = Utils.Track1(mds, spreaders.array[count++], out logmin);

                currentTime = Utils.computeDay(count, queryArray);
                //追踪核心算法
                DynamicArray<Interval> result = Utils.Track(mds, tempArray.array[count], spreaders, out logmin, personArray, currentTime+1);
                logmin = Utils.reportContact(result, logmin, personArray);
                this.CurrentState.Content = logmin;
                log += logmin + "\n";

                //更新界面颜色：
                personArray = updatePersonColor(personArray, result);
                updateSubpages();
             
                updateLocColor(spreaders.array[tempArray.array[count].getId()].locName.array[0], tempArray.array[count].getId());
                this.TimeSlider.Value = tempArray.array[count].getStart();

                count++;
            }

            this.btnSearch.IsEnabled = false;
            this.btnResume.IsEnabled = true;

        }

        //继续按钮的点击事件
        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            if (count >= tempArray.count)
            {
                this.btnSearch.IsEnabled = true;
                this.btnResume.IsEnabled = false;
                this.CurrentState.Content = "结束";
                pageReload();
            }
            else
            {
                string logmin = "";
                currentTime = Utils.computeDay(count, queryArray);
                DynamicArray<Interval> result = Utils.Track(mds, tempArray.array[count], spreaders, out logmin, personArray, currentTime+1);
                logmin = Utils.reportContact(result, logmin, personArray);
                this.CurrentState.Content = logmin;
                log += logmin + '\n';
                //更新颜色信息：
                personArray = updatePersonColor(personArray, result);
                updateSubpages();//更新页面
                int nowLoc = spreaders.array[tempArray.array[count].getId()].findLoc(tempArray.array[count].getStart() + 1440 * currentTime, tempArray.array[count].getEnd() + 1440 * currentTime);
                int lastLoc = Utils.findLastLoc(count, tempArray.array[count].getId(), tempArray, spreaders.array[tempArray.array[count].getId()], currentTime, nowLoc);
                Console.WriteLine("lastLoc:" + lastLoc);
                Console.WriteLine("nowLoc :" + nowLoc);
                updateLocColor(nowLoc, tempArray.array[count].getId());
                if(lastLoc != -1 && nowLoc != -1)
                {
                    DrawTrace(lastLoc, nowLoc, tempArray.array[count].getId());
                }
                count++;
            }
        }

        /*
         下控制面板应用函数
         */
        /*
            添加患者信息
         */

        //添加患者：
        private void btnCreateSpreader_Click(object sender, RoutedEventArgs e)
        {
            this.btnCreateSpreader.IsEnabled = false;
            this.nameTextBox.IsEnabled = true;
            this.teleTextBox.IsEnabled = true;
            this.deptTextBox.IsEnabled = true;
            this.addrTextBox.IsEnabled = true;
            this.btnCreateDone.IsEnabled = true;
            this.comboPlace.IsEnabled = true;
            this.startHour.IsEnabled = true;
            this.startMin.IsEnabled = true;
            this.leftHour.IsEnabled = true;
            this.leftMin.IsEnabled = true;
            this.addInfo.IsEnabled = true;
            this.delInfo.IsEnabled = true;
        }

        //功能：添加患者人物信息：
        private void addInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                int loc = Utils.saveFindStringIndex(this.comboPlace.Text, Utils.places);
                int startTime = Utils.ConvertTime2Int(int.Parse(this.startHour.Text), int.Parse(this.startMin.Text));
                int leftTime = Utils.ConvertTime2Int(int.Parse(this.leftHour.Text), int.Parse(this.leftMin.Text));
                int day = int.Parse(this.comboDay.Text);
                //如果离开时间小于进入时间，报错
                if (leftTime - startTime <= 0)
                    throw new Exception();
                Console.WriteLine(startTime + "~" + leftTime);                
                //MessageBox.Show("成功添加任务信息！");
                string briefinfo = this.nameTextBox.Text.ToString() + " " + Utils.places[loc] + " 第"+day+"天 " + Utils.ConvertInt2Time(startTime) + "~" + Utils.ConvertInt2Time(leftTime);
                this.spreaderInfo.Items.Add(briefinfo);
            }
            catch
            {
                MessageBox.Show("添加失败!");
            }
        }

        //功能：删除患者任务信息
        private void delInfo_Click(object sender, RoutedEventArgs e)
        {
            if(this.spreaderInfo.Items.Count == 0)
            {
                MessageBox.Show("患者列表为空");
            }
            else
            {
                int index = this.spreaderInfo.SelectedIndex;
                if (index < 0)
                {
                    MessageBox.Show("请选择要删除的信息");
                    return;
                }
                this.spreaderInfo.Items.RemoveAt(index);
                MessageBox.Show("成功删除该患者信息");
            }
        }
        
        //确认新建：
        private void btnCreateDone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nameSick = this.nameTextBox.Text.ToString();
                string addrSick = this.addrTextBox.Text.ToString();
                string deptSick = this.deptTextBox.Text.ToString();
                string teleSick = this.teleTextBox.Text.ToString();
                if (nameSick == "" || addrSick == "" || deptSick == "" || teleSick == "")
                    throw new Exception();
                int numLoc = this.spreaderInfo.Items.Count;
                DynamicArray<int> startTime = new DynamicArray<int>();
                DynamicArray<int> leftTime = new DynamicArray<int>();
                DynamicArray<int> locName = new DynamicArray<int>();

                DynamicArray<Interval> temp = new DynamicArray<Interval>();

                for(int i = 0; i < numLoc; i++)
                {
                    string info = this.spreaderInfo.Items[i].ToString();
                    char[] chs = { ' ' };
                    char[] chs2 = { '~' };
                    string[] items = info.Split(chs, options: StringSplitOptions.RemoveEmptyEntries);
                    string place = items[1];
                    int day = items[2].ToCharArray()[1] - '0';
                    Console.WriteLine("day: "+day);
                    string time = items[3];
                    string[] timeItems = time.Split(chs2, options: StringSplitOptions.RemoveEmptyEntries);
                    string start = timeItems[0];
                    string left = timeItems[1];
                    int loc = Utils.saveFindStringIndex(place, Utils.places);
/*                    startTime.add(Utils.ConvertStringDay2Int(day, start));
                    leftTime.add(Utils.ConvertStringDay2Int(day, left));
                    locName.add(loc);*/
                    Console.WriteLine("startTime: "+start);
                    Console.WriteLine("leftTime: "+left);
                    temp.add(new Interval(Utils.ConvertStringDay2Int(day, start), Utils.ConvertStringDay2Int(day, left), loc));
                }
                temp = Utils.sortByInterval(temp);
                for(int i = 0; i < temp.count; i++)
                {
                    startTime.add(temp.array[i].getStart());
                    leftTime.add(temp.array[i].getEnd());
                    locName.add(temp.array[i].getId());
                }

                Person spreader = new Person(Utils.spreaderId++, nameSick, deptSick, addrSick, teleSick, false, true, true, numLoc, startTime, leftTime, locName);
                spreaders.add(spreader);
                //生成queryArray,共3个：
                queryArray = Utils.generateQueryArray(spreaders);

                //给queryArray排序：
                for(int i = 0; i < 3; i++)
                {
                    queryArray[i] = Utils.sortByInterval(queryArray[i]);
                }
                
                //将记录显示出来
                PrintPersonOnSite(spreader);
                this.nameTextBox.Text = "";
                this.teleTextBox.Text = "";
                this.deptTextBox.Text = "";
                this.addrTextBox.Text = "";
                this.comboPlace.Text = "";
                this.startHour.Text = "";
                this.startMin.Text = "";
                this.leftHour.Text = "";
                this.leftMin.Text = "";
                this.spreaderInfo.Items.Clear();
                this.comboDay.Text = "";

                this.btnCreateSpreader.IsEnabled = false;
                this.nameTextBox.IsEnabled = false;
                this.teleTextBox.IsEnabled = false;
                this.deptTextBox.IsEnabled = false;
                this.addrTextBox.IsEnabled = false;
                this.btnCreateDone.IsEnabled = false;
                this.comboPlace.IsEnabled = false;
                this.startHour.IsEnabled = false;
                this.startMin.IsEnabled = false;
                this.leftHour.IsEnabled = false;
                this.leftMin.IsEnabled = false;
                this.addInfo.IsEnabled = false;
                this.delInfo.IsEnabled = false;
                this.btnCreateSpreader.IsEnabled = true;


            }
            catch
            {
                MessageBox.Show("添加患者失败!");
            }
        }

        /*
            添加人员信息：
         */

        //添加人员：
        private void createPerson_Click(object sender, RoutedEventArgs e)
        {
            this.btnCreatePerson.IsEnabled = false;
            this.nameTextBox2.IsEnabled = true;
            this.teleTextBox2.IsEnabled = true;
            this.deptTextBox2.IsEnabled = true;
            this.addrTextBox2.IsEnabled = true;
            this.btnCreateDone2.IsEnabled = true;
            this.comboPlace2.IsEnabled = true;
            this.startHour2.IsEnabled = true;
            this.startMin2.IsEnabled = true;
            this.leftHour2.IsEnabled = true;
            this.leftMin2.IsEnabled = true;
            this.addInfo2.IsEnabled = true;
            this.delInfo2.IsEnabled = true;
        }

        //功能：添加人员去过地点信息：
        private void addInfo_Click2(object sender, RoutedEventArgs e)
        {
            try
            {
                int loc = Utils.saveFindStringIndex(this.comboPlace2.Text, Utils.places);
                int startTime = Utils.ConvertTime2Int(int.Parse(this.startHour2.Text), int.Parse(this.startMin2.Text));
                int leftTime = Utils.ConvertTime2Int(int.Parse(this.leftHour2.Text), int.Parse(this.leftMin2.Text));
                //如果离开时间小于进入时间，报错
                if (leftTime - startTime <= 0)
                    throw new Exception();
                Console.WriteLine(startTime + "~" + leftTime);
                //MessageBox.Show("成功添加任务信息！");
                string briefinfo = this.nameTextBox2.Text.ToString() + " " + Utils.places[loc] + " " + Utils.ConvertInt2Time(startTime) + "~" + Utils.ConvertInt2Time(leftTime);
                this.personInfo.Items.Add(briefinfo);
            }
            catch
            {
                MessageBox.Show("添加失败!");
            }
        }

        //功能：删除人员去过地点信息：
        private void delInfo_Click2(object sender, RoutedEventArgs e)
        {
            if (this.personInfo.Items.Count == 0)
            {
                MessageBox.Show("患者列表为空");
            }
            else
            {
                int index = this.personInfo.SelectedIndex;
                if (index < 0)
                {
                    MessageBox.Show("请选择要删除的信息");
                    return;
                }
                this.personInfo.Items.RemoveAt(index);
                MessageBox.Show("成功删除该患者信息");
            }
        }

        //确认新建：
        private void btnCreateDone2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = this.nameTextBox2.Text.ToString();
                string addr = this.addrTextBox2.Text.ToString();
                string dept = this.deptTextBox2.Text.ToString();
                string tele = this.teleTextBox2.Text.ToString();
                if (name == "" || addr == "" || dept == "" || tele == "")
                    throw new Exception();
                int numLoc = this.personInfo.Items.Count;
                bool isProtected = this.isPro.IsChecked.Value;
                DynamicArray<int> startTime = new DynamicArray<int>();
                DynamicArray<int> leftTime = new DynamicArray<int>();
                DynamicArray<int> locName = new DynamicArray<int>();
                for (int i = 0; i < numLoc; i++)
                {
                    string info = this.personInfo.Items[i].ToString();
                    char[] chs = { ' ' };
                    char[] chs2 = { '~' };
                    string[] items = info.Split(chs, options: StringSplitOptions.RemoveEmptyEntries);
                    string place = items[1];
                    string time = items[2];
                    string[] timeItems = time.Split(chs2, options: StringSplitOptions.RemoveEmptyEntries);
                    string start = timeItems[0];
                    string left = timeItems[1];
                    int loc = Utils.saveFindStringIndex(place, Utils.places);
                    startTime.add(Utils.ConvertString2Int(start));
                    leftTime.add(Utils.ConvertString2Int(left));
                    locName.add(loc);

                }
                Person person = new Person(Utils.personId++, name, dept, addr, tele, isProtected, false, false, numLoc, startTime, leftTime, locName);
                personArray.add(person);
                //加入到mds中：
                mds.addPerson(person);
                //将记录显示出来
                updateSubpages();

                this.nameTextBox2.Text = "";
                this.teleTextBox2.Text = "";
                this.deptTextBox2.Text = "";
                this.addrTextBox2.Text = "";
                this.comboPlace2.Text = "";
                this.startHour2.Text = "";
                this.startMin2.Text = "";
                this.leftHour2.Text = "";
                this.leftMin2.Text = "";
                this.personInfo.Items.Clear();

                this.btnCreatePerson.IsEnabled = false;
                this.nameTextBox2.IsEnabled = false;
                this.teleTextBox2.IsEnabled = false;
                this.deptTextBox2.IsEnabled = false;
                this.addrTextBox2.IsEnabled = false;
                this.btnCreateDone2.IsEnabled = false;
                this.comboPlace2.IsEnabled = false;
                this.startHour2.IsEnabled = false;
                this.startMin2.IsEnabled = false;
                this.leftHour2.IsEnabled = false;
                this.leftMin2.IsEnabled = false;
                this.addInfo2.IsEnabled = false;
                this.delInfo2.IsEnabled = false;
                this.btnCreatePerson.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show("添加患者失败!");
            }
        }



        /* 
         在页面上显示部分
         */
        //地点的位置：
        double[] locX = { 432.519, 649.124, 672.115, 540.217, 624.922, 313.933, 129.999, 345.393 };
        double[] locY = { 90.486, 159.62, 312.091, 389.537, 504, 504, 422.923, 243.83 };

        public void updateSubpages()
        {
            //清除subpages：
            this.Stack7.Children.Clear();
            this.Stack6.Children.Clear();
            this.Stack5.Children.Clear();
            this.Stack4.Children.Clear();
            this.Stack3.Children.Clear();
            this.Stack2.Children.Clear();
            this.Stack1.Children.Clear();
            this.Stack0.Children.Clear();
            //制作subpages：
            //导入树中结点
            for (int i = 0; i < mds.it.Length; i++)
            {
                showInfoInPopup(i);
            }
            //导入患者
            for (int i = 0; i < spreaders.count; i++)
            {
                PrintPersonOnSite(spreaders.array[i]);
            }
        }

        //subpages popup显示
        private void btn7_Click(object sender, RoutedEventArgs e)
        {
            //this.Stack7.Children.Clear();
            this.pop7.IsOpen = false;
            this.pop7.IsOpen = true;
        }

        private void btn6_Click(object sender, RoutedEventArgs e)
        {
            //this.Stack6.Children.Clear();
            this.pop6.IsOpen = false;
            this.pop6.IsOpen = true;
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            //this.Stack5.Children.Clear();
            this.pop5.IsOpen = false;
            this.pop5.IsOpen = true;
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {
            //this.Stack4.Children.Clear();
            this.pop4.IsOpen = false;
            this.pop4.IsOpen = true;
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
           // this.Stack3.Children.Clear();
            this.pop3.IsOpen = false;
            this.pop3.IsOpen = true;
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            //this.Stack2.Children.Clear();
            this.pop2.IsOpen = false;
            this.pop2.IsOpen = true;
        }

        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            //this.Stack1.Children.Clear();
            this.pop1.IsOpen = false;
            this.pop1.IsOpen = true;
        }

        private void btn0_Click(object sender, RoutedEventArgs e)
        {
            //this.Stack0.Children.Clear();
            this.pop0.IsOpen = false;
            this.pop0.IsOpen = true;
        }

        //功能：更新人员状态
        public DynamicArray<Person> updatePersonColor(DynamicArray<Person> personArray, DynamicArray<Interval> result)
        {
            for(int i = 0; i < result.count; i++)
            {
                if(personArray.array[result.array[i].getId()].isProtected == false)
                    personArray.array[result.array[i].getId()].isAffected = true;
                int loc = personArray.array[result.array[i].getId()].findLoc(result.array[i].getStart(), (result.array[i].getEnd()));
                //updateLocColor(loc, result.array[i].getId());
            }
            return personArray;
        }

        //功能：更新该地点color
        public void updateLocColor(int loc, int id)
        {
            if(id == 0)
            {
                switch (loc)
                {
                    case (0): this.img0.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    case (1): this.img1.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    case (2): this.img2.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    case (3): this.img3.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    case (4): this.img4.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    case (5): this.img5.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    case (6): this.img6.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    case (7): this.img7.Source = new BitmapImage(new Uri("map-1.png", UriKind.Relative)); break;
                    default: break;
                }
            }
            else if (id == 1)
            {
                switch (loc)
                {
                    case (0): this.img0.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    case (1): this.img1.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    case (2): this.img2.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    case (3): this.img3.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    case (4): this.img4.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    case (5): this.img5.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    case (6): this.img6.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    case (7): this.img7.Source = new BitmapImage(new Uri("map-4.png", UriKind.Relative)); break;
                    default: break;
                }
            }
            else
            {
                switch (loc)
                {
                    case (0): this.img0.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    case (1): this.img1.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    case (2): this.img2.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    case (3): this.img3.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    case (4): this.img4.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    case (5): this.img5.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    case (6): this.img6.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    case (7): this.img7.Source = new BitmapImage(new Uri("map-5.png", UriKind.Relative)); break;
                    default: break;
                }
            }


        }

        //时间线（待定）
        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)e.NewValue;
            this.TimePlate.Content = Utils.ConvertInt2Time(value);
        }


        //功能：将一个地点的当日人员信息显示在subpage上（popup）
        public void showInfoInPopup(int loc)
        {
            IntervalTree ait = mds.it[loc];
            helpMakeLabel(ait.root, loc, "person", -1);

        }

        //功能：用先序遍历的方式遍历每个结点，以创建每个结点的显示信息：
        public void helpMakeLabel(Interval tmp,int loc, string type, int id)
        {
            if (tmp == null)
            {
                return;
            }

            if (tmp.getLeft() != null)
            {
                helpMakeLabel(tmp.getLeft(), loc, type, -1);
            }
            //画：
            Label label = new Label();
            Person p;
            if(type == "person")
            {
                p = personArray.array[tmp.getId()];
                label.FontSize = 10;
                label.Content = p.name.ToString() + " 逗留时间:" + "第" + Convert.ToInt32(currentTime + 1) + "天"+ Utils.ConvertInt2Time(tmp.getStart()) + "-" + Utils.ConvertInt2Time(tmp.getEnd()) +
                    " 联系电话:" + p.tele + " 家庭住址:" + p.addr + "单位地址" + p.dept;
                label.Width = double.NaN;
            }
            else
            {
                p = spreaders.array[tmp.getId()];
                label.FontSize = 10;
                label.Content = p.name.ToString() + " 逗留时间:" + "第"+ Convert.ToInt32(tmp.getStart()/1440+1) + "天"+Utils.ConvertInt2Time(tmp.getStart()%1440) + "-" + Utils.ConvertInt2Time(tmp.getEnd()%1440) +
                    " 联系电话:" + p.tele + " 家庭住址:" + p.addr + "单位地址" + p.dept;
                label.Width = double.NaN;
            }
            
            if(p.isSpreader == true)
            {
                if(id==0) label.Background = new SolidColorBrush(Color.FromArgb(255, 190, 0, 47));
                else if (id == 1) label.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 255));
                else label.Background = new SolidColorBrush(Color.FromArgb(255, 128, 64, 0));
            }
            else if (p.isAffected == true)
            {
                label.Background = new SolidColorBrush(Color.FromArgb(255, 255, 97, 0));

            }
            else if (p.isProtected == true)
            {
                label.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            }
            else if (p.isProtected == false)
            {
                label.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
            }
            switch (loc)
            {
                case (0): this.Stack0.Children.Add(label); break;
                case (1): this.Stack1.Children.Add(label); break;
                case (2): this.Stack2.Children.Add(label); break;
                case (3): this.Stack3.Children.Add(label); break;
                case (4): this.Stack4.Children.Add(label); break;
                case (5): this.Stack5.Children.Add(label); break;
                case (6): this.Stack6.Children.Add(label); break;
                case (7): this.Stack7.Children.Add(label); break;
                default: break;
            }



            //画结束

            if (tmp.getRight() != null)
            {
                helpMakeLabel(tmp.getRight(), loc, type, -1);
            }
        }

        //功能：将给定任务加在每个地点的信息上：
        public void PrintPersonOnSite(Person p)
        {
            for(int i = 0; i < p.numLoc; i++)
            {
                helpMakeLabel(new Interval(p.startTime.array[i], p.leftTime.array[i], p.id), p.locName.array[i], "spreaders", p.id);
            }
        }

        //将log写入问价路径为savePath的地方
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dateStr = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
                Utils.WriteFile("密接者"+dateStr+"查询记录", savePath, log);
                MessageBox.Show("保存成功！");
            }
            catch
            {
                MessageBox.Show("保存失败！");
            }
        }
        
        //功能：允许用户添加新发现的某时段某地驻足人员
        private void addPerson_Click(object sender, RoutedEventArgs e)
        {

        }

        //功能：画线
        public void DrawTrace(int loc1, int loc2, int spreadId)
        {
            Line myLine = new Line();
            if (spreadId == 0)
                myLine.Stroke = Brushes.Red;
            else if (spreadId == 1)
                myLine.Stroke = Brushes.Purple;
            else
                myLine.Stroke = Brushes.Brown;

            myLine.X1 = locX[loc1];
            myLine.Y1 = locY[loc1];
            myLine.X2 = locX[loc2];
            myLine.Y2 = locY[loc2];
            myLine.StrokeThickness = 5;
            this.canvas.Children.Add(myLine);
        }

        //功能：查询一次之后，重新加载页面
        public void pageReload()
        {
            this.InitializeComponent();
            this.canvas.Children.Clear();
            count = 0; //查询次数
            spreaders = new DynamicArray<Person>(); //存储的患者们
            personArray = new DynamicArray<Person>(); //存储的人员们
            queryArray = new DynamicArray<Interval>[3]; //查询队列，时间从小到大排序，由spreaders生成
            for (int i = 0; i < 3; i++)
            {
                queryArray[i] = new DynamicArray<Interval>();
            }
            openPath = ""; //打开文件的路径
            savePath = ""; //保存文件的路径
            log = ""; //记录查询的信息
            this.saveFile.Text = "";
            this.openFile.Text = "";
            mds = new MainDS(); //主数据结构
            Utils.personId = 0;
            Utils.spreaderId = 0;
            this.img0.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative)); 
            this.img1.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative)); 
            this.img2.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative)); 
            this.img3.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative)); 
            this.img4.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative)); 
            this.img5.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative)); 
            this.img6.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative)); 
            this.img7.Source = new BitmapImage(new Uri("map-2.png", UriKind.Relative));
            this.TimeSlider.Value = 0;
            updateSubpages();
            currentTime = 0;
        }
    }
}