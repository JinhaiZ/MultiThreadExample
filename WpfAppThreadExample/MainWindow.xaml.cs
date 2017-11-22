using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClassLibraryPremier;
using WpfAppliTh;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace WpfAppThreadExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LinkedList<ThreadNode> listThread;
        ObservableCollection<ThreadViewItem> threadViewList = new ObservableCollection<ThreadViewItem>();

        public MainWindow()
        {
            InitializeComponent();
            // important for databinding
            this.DataContext = this;
            listThread = new LinkedList<ThreadNode>();
        }
        // wrapper of threadViewList, the wrapper is used to binding the
        // ListView to ThreadViewList
        public ObservableCollection<ThreadViewItem> ThreadViewList
        {
            get { return threadViewList; }
        }

        private void startBallonMethod()
        {
            WindowBallon wb = new WindowBallon();
            wb.Show();
        }

        private void methodThread()
        {
            this.Dispatcher.Invoke(startBallonMethod);
        }

        private void addItemToList(string type, Thread t)
        {
            //Debug.WriteLine("ID: " + t.ManagedThreadId + "State" + t.ThreadState);
            ThreadViewItem threadViewItem = new ThreadViewItem(listThread.Count, type, t.ManagedThreadId, t.ThreadState.ToString());
            //threadViewList and its items are binding on two directions, use with ease
            threadViewList.Add(threadViewItem);
            //Debug.WriteLine(threadViewList.Count);
            listThread.AddLast(new ThreadNode(t, threadViewItem));
        }
        private void startBallon_Click(object sender, RoutedEventArgs e)
        {

            Thread t = new Thread(() => methodThread());
            t.Start();
            addItemToList("ballon", t);

        }

        private void startPremier_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(() => NombrePremier.Premier(1));
            t.Start();
            addItemToList("premier", t);
        }

        private void stopLastBallon_Click(object sender, RoutedEventArgs e)
        {


        }

        private void StopLastPremier_Click(object sender, RoutedEventArgs e)
        {
            if (listThread.Count <= 0 || ThreadNode.getPremierCount() <= 0)
                MessageBox.Show("No running thread for premier.exe", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                for (int i = listThread.Count - 1; i >= 0; i--)
                {
                    if (listThread.ElementAt(i).threadViewItem.Type == "premier")
                    {
                        //get last node of type premier from the linkedlist
                        ThreadNode toRemove = listThread.ElementAt(i);
                        toRemove.thread.Abort();
                        threadViewList.Remove(toRemove.threadViewItem);
                        ThreadNode.setPremierCount(ThreadNode.getPremierCount() - 1);
                        //setViewCounters(ThreadNode.getBallonCount(), ThreadNode.getPremierCount());
                        listThread.Remove(toRemove);
                        break;
                    }
                }
            }
        }


        private void StopLastThread_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopAllThread_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class ThreadViewItem
    {
        public ThreadViewItem(int ThreadNum, string Type, int ID, string State)
        {
            this.ThreadNum = ThreadNum;
            this.Type = Type;
            this.ID = ID;
            this.State = State;
        }
        public int ThreadNum { get; set; }
        public string Type { get; set; }
        public int ID { get; set; }
        public string State { get; set; }
    }

    public class ThreadNode
    {
        public Thread thread;
        public ThreadViewItem threadViewItem;
        public static int ballonCount = 0;
        public static int premierCount = 0;

        public ThreadNode(Thread thread, ThreadViewItem threadViewItem)
        {
            this.thread = thread;
            this.threadViewItem = threadViewItem;
            if (threadViewItem.Type == "ballon")
                ballonCount += 1;
            else
                premierCount += 1;
        }

        public static int getBallonCount()
        {
            return ballonCount;
        }
        public static int getPremierCount()
        {
            return premierCount;
        }
        public static void setBallonCount(int count)
        {
            ballonCount = count;
        }
        public static void setPremierCount(int count)
        {
            premierCount = count;
        }
    }
}
