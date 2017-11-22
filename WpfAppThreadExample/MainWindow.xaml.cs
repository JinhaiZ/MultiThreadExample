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
        public ObservableCollection<ThreadViewItem> threadViewList = new ObservableCollection<ThreadViewItem>();

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

        private void setViewCounters(int ballonCount, int premierCount)
        {
            ballonCountView.Text = ballonCount.ToString();
            premierCountView.Text = premierCount.ToString();
            countView.Text = (ballonCount + premierCount).ToString();
        }

        private void addItemToList(string type, Thread t)
        {
            //Debug.WriteLine("ID: " + t.ManagedThreadId + "State" + t.ThreadState);
            ThreadViewItem threadViewItem = new ThreadViewItem(listThread.Count, type, t.ManagedThreadId, t.ThreadState.ToString());
            //threadViewList and its items are binding on two directions, use with ease
            threadViewList.Add(threadViewItem);
            //Debug.WriteLine(threadViewList.Count);
            listThread.AddLast(new ThreadNode(t, threadViewItem));
            //update counters
            setViewCounters(ThreadNode.getBallonCount(), ThreadNode.getPremierCount());
        }

        private void startBallon_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(() =>
            {
                WindowBallon wb = new WindowBallon();
                wb.Show();

                wb.Closed += (sender2, e2) =>
                    wb.Dispatcher.InvokeShutdown();

                wb.Closed += (sender2, e2) =>
                    checkClosedThread();
                    

                System.Windows.Threading.Dispatcher.Run();
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            Debug.WriteLine("isalive: "+t.IsAlive);
            // inspired by the post : https://stackoverflow.com/questions/1111369/how-do-i-create-and-show-wpf-windows-on-separate-threads
            addItemToList("ballon", t);

        }

        private void startPremier_Click(object sender, RoutedEventArgs e)
        {
            ThreadStart ts = new ThreadStart(() => NombrePremier.Premier(ThreadNode.getPremierCount()));
            ts += () => 
            {
                checkClosedThread();
            };

            Thread t = new Thread(ts);
            t.Start();
            addItemToList("premier", t);
        }
        private void deleteItemFromList(string type)
        {
            for (int i = listThread.Count - 1; i >= 0; i--)
            {
                if (listThread.ElementAt(i).threadViewItem.Type == type)
                {
                    //get last node of type premier from the linkedlist
                    ThreadNode toRemove = listThread.ElementAt(i);
                    toRemove.thread.Abort();
                    threadViewList.Remove(toRemove.threadViewItem);
                    if (type == "premier")
                    {
                        ThreadNode.setPremierCount(ThreadNode.getPremierCount() - 1);
                        //Debug.WriteLine("Premier Count: " + ThreadNode.getPremierCount().ToString());
                    }
                    else
                    {
                        ThreadNode.setBallonCount(ThreadNode.getBallonCount() - 1);
                        //Debug.WriteLine("Ballon Count: " + ThreadNode.getBallonCount().ToString());
                        Debug.WriteLine("isalive: " + toRemove.thread.IsAlive);
                    }
                    //update counters
                    setViewCounters(ThreadNode.getBallonCount(), ThreadNode.getPremierCount());
                    listThread.Remove(toRemove);
                    break;
                }
            }
        }
        private void stopAllThread()
        {
            foreach (ThreadNode tN in listThread)
            {
                try
                {
                    tN.thread.Abort();
                }
                catch (ThreadStateException)
                {
                    Debug.WriteLine("exception !!!!");
                    tN.thread.Resume();
                }
                threadViewList.Remove(tN.threadViewItem);
            }
            ThreadNode.setBallonCount(0);
            ThreadNode.setPremierCount(0);
            listThread = new LinkedList<ThreadNode>();
            setViewCounters(0, 0);
        }

        private void stopLastBallon_Click(object sender, RoutedEventArgs e)
        {
            if (listThread.Count <= 0 || ThreadNode.getBallonCount() <= 0)
                MessageBox.Show("No running thread for ballon.exe", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                deleteItemFromList("ballon");
            }
        }

        private void StopLastPremier_Click(object sender, RoutedEventArgs e)
        {
            if (listThread.Count <= 0 || ThreadNode.getPremierCount() <= 0)
                MessageBox.Show("No running thread for premier.exe", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                deleteItemFromList("premier");
            }
        }

        private void StopLastThread_Click(object sender, RoutedEventArgs e)
        {
            if (listThread.Count <= 0)
                MessageBox.Show("No running thread", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                listThread.Last().thread.Abort();
                threadViewList.Remove(listThread.Last().threadViewItem);
                if (listThread.Last().threadViewItem.Type == "ballon")
                    ThreadNode.setBallonCount(ThreadNode.getBallonCount() - 1);
                else
                    ThreadNode.setPremierCount(ThreadNode.getPremierCount() - 1);
                setViewCounters(ThreadNode.getBallonCount(), ThreadNode.getPremierCount());
                listThread.RemoveLast();
            }
        }

        private void StopAllThread_Click(object sender, RoutedEventArgs e)
        {
            if (listThread.Count <= 0)
                MessageBox.Show("No running thread", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                stopAllThread();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want quit?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                stopAllThread();
            else
                e.Cancel = true;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Suspend_Click(object sender, RoutedEventArgs e)
        {
            foreach (ThreadNode tN in listThread)
            {
                tN.threadViewItem.State = "Suspending";
                tN.thread.Suspend();
            }
            /*foreach (ThreadViewItem tN in threadViewList)
            {
                Debug.WriteLine(tN.State);
            }*/
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            foreach (ThreadNode tN in listThread)
            {
                tN.thread.Resume();
                tN.threadViewItem.State = "Running";
            }
        }

        private void checkClosedThread()
        {
            Debug.WriteLine("checkClosedThread");
            foreach (ThreadNode pN in listThread)
            {
                Debug.WriteLine("thread type {0}, id {1}, isalive {2}, status {3} ", 
                    pN.threadViewItem.Type, pN.threadViewItem.ID, pN.thread.IsAlive.ToString(), pN.thread.ThreadState.ToString());
                if (!pN.thread.IsAlive)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        threadViewList.Remove(pN.threadViewItem);
                        if (pN.threadViewItem.Type == "ballon")
                            ThreadNode.setBallonCount(ThreadNode.getBallonCount() - 1);
                        else
                            ThreadNode.setPremierCount(ThreadNode.getPremierCount() - 1);
                        setViewCounters(ThreadNode.getBallonCount(), ThreadNode.getPremierCount());
                        listThread.Remove(pN);
                    });
                    break;
                }
            }
            /*
            foreach (ThreadNode pN in listThread)
            {
                Debug.WriteLine("thread type {0}, id {1}, isalive {2}, status {3} ",
                    pN.threadViewItem.Type, pN.threadViewItem.ID, pN.thread.IsAlive.ToString(), pN.thread.ThreadState.ToString());
            }*/
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
