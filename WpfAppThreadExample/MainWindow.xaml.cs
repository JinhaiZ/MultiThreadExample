using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
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
        // déclarer une liste pour la gestion des threads
        private LinkedList<ThreadNode> listThread;
        // déclarer une collection pour afficher les informations de thread
        public ObservableCollection<ThreadViewItem> threadViewList = new ObservableCollection<ThreadViewItem>();

        public MainWindow()
        {
            InitializeComponent();
            // définir DataContext, il est important pour la data binding entre ListView UI et ThreadViewList
            this.DataContext = this;
            listThread = new LinkedList<ThreadNode>();
        }
        // wrapper de threadViewList
        public ObservableCollection<ThreadViewItem> ThreadViewList
        {
            get { return threadViewList; }
        }
        // fonction qui définit le contenu des TextBlock UI comme ballonCountView, premierCountView et countView
        private void setViewCounters(int ballonCount, int premierCount)
        {
            ballonCountView.Text = ballonCount.ToString();
            premierCountView.Text = premierCount.ToString();
            countView.Text = (ballonCount + premierCount).ToString();
        }
        // ajouter un thread dans le ListThread et dans le même temps met à jour UI ListView 
        private void addItemToList(string type, Thread t)
        {
            //Debug.WriteLine("ID: " + t.ManagedThreadId + "State" + t.ThreadState);
            ThreadViewItem threadViewItem = new ThreadViewItem(listThread.Count, type, t.ManagedThreadId, t.ThreadState.ToString());
            //threadViewList et ses éléments sont binding dans deux sens, utilisez avec facilité
            threadViewList.Add(threadViewItem);
            //Debug.WriteLine(threadViewList.Count);
            listThread.AddLast(new ThreadNode(t, threadViewItem));
            //mise à jour les compteurs
            setViewCounters(ThreadNode.getBallonCount(), ThreadNode.getPremierCount());
        }
        // fonction qui sera appelée lorsque le sous-menu start ballon est cliqué
        private void startBallon_Click(object sender, RoutedEventArgs e)
        {
            // créer un thread pour afficher le WindowBallon, bien que WindowBallon puisse être exécuté sans thread, 
            // le thread joue ici un rôle wrapper afin que les informations relatives au thread puissent être utilisées comme l'ID de thread
            ThreadStart ts = new ThreadStart(() =>
            {
                WindowBallon wb = new WindowBallon();
                wb.Show();
                // la valeur attribué au id serait son id
                int id = ThreadNode.getCount();
                // ajouter un event handler qui va être appelé lorsque WindowBallon est fermé, mettre à jour ListView
                wb.Closed += (sender2, e2) =>
                    checkClosedThread(id);
                // ajouter un event handler qui va être appelé lorsque WindowBallon est fermé, terminer le distributeur lorsque la fenêtre se ferme
                wb.Closed += (sender2, e2) =>
                    wb.Dispatcher.InvokeShutdown();

                System.Windows.Threading.Dispatcher.Run();
            });
            Thread t = new Thread(ts);
            // Le thread appelant doit être STA, car les composants de l'interface utilisateur requièrent ça
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            Debug.WriteLine("isalive: "+t.IsAlive);
            // ajouter un thread à listThread et également mettre à jour ViewList UI
            addItemToList("ballon", t);

        }
        // fonction qui sera appelée lorsque le sous-menu start premier est cliqué
        private void startPremier_Click(object sender, RoutedEventArgs e)
        {
            ThreadStart ts = new ThreadStart(() => NombrePremier.Premier(ThreadNode.getPremierCount()));
            // pas possible d'arrêter un thread de type premier par cliquer un croix rouge, donc
            // les lignes suivantes sont commentées
            /*
            ts += () => 
            {
                checkClosedThread();
            };
            */
            Thread t = new Thread(ts);
            t.Start();
            addItemToList("premier", t);
        }
        // fontion qui gerè la suppression des élement dans la liste listThread et la liste threadViewList
        private void deleteItemFromList(string type)
        {
            for (int i = listThread.Count - 1; i >= 0; i--)
            {
                if (listThread.ElementAt(i).threadViewItem.Type == type)
                {
                    // get last node of type premier from the linkedlist
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
        // arrêter tous les threads
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
        // // fonction qui sera appelée lorsque le sous-menu stop last thread est cliqué
        private void StopLastThread_Click(object sender, RoutedEventArgs e)
        {
            if (listThread.Count <= 0)
                MessageBox.Show("No running thread", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                // arrêter le dérnier thread
                listThread.Last().thread.Abort();
                // supprimer l'élément dépuis la liste UI threadViewList
                threadViewList.Remove(listThread.Last().threadViewItem);
                // mise à jour les compteurs également
                if (listThread.Last().threadViewItem.Type == "ballon")
                    ThreadNode.setBallonCount(ThreadNode.getBallonCount() - 1);
                else
                    ThreadNode.setPremierCount(ThreadNode.getPremierCount() - 1);
                setViewCounters(ThreadNode.getBallonCount(), ThreadNode.getPremierCount());
                // supprimer l'élément dépuis la liste listThread
                listThread.RemoveLast();
            }
        }
        // fonction qui sera appelée lorsque le sous-menu stop all thread est cliqué
        private void StopAllThread_Click(object sender, RoutedEventArgs e)
        {
            if (listThread.Count <= 0)
                MessageBox.Show("No running thread", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                stopAllThread();
        }
        // fonction qui sera appelée lorsque l'utilisateur clique le croix rouge
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want quit?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                stopAllThread();
            else
                e.Cancel = true;
        }
        // fonction qui sera appelée lorsque le menu quit est cliqué
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        // fonction qui sera appelée lorsque le sous-menu suspend est cliqué
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
        // fonction qui sera appelée lorsque le sous-menu resume est cliqué
        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            foreach (ThreadNode tN in listThread)
            {
                tN.thread.Resume();
                tN.threadViewItem.State = "Running";
            }
        }
        // vérifier les threads fermées et les supprimer
        private void checkClosedThread(int id)
        {
            Debug.WriteLine("checkClosedThread ID: {0}", id);
            foreach (ThreadNode pN in listThread)
            {
                Debug.WriteLine("thread type {0}, id {1}, isalive {2}, status {3} ", 
                    pN.threadViewItem.Type, pN.getID(), pN.thread.IsAlive.ToString(), pN.thread.ThreadState.ToString());
                if (pN.id == id)
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
    // définir l'élément stocké dans la liste threadViewList
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
    // définir l'élément stocké dans la liste listThread
    public class ThreadNode
    {
        public Thread thread;
        public ThreadViewItem threadViewItem;
        public static int ballonCount = 0;
        public static int premierCount = 0;
        // count est un attribut de classe, c'est utilisé pour attribuer le valuer au attribut d'instance id
        // il est initialisé par -1, donc le premier id est 0
        public static int count = -1;
        // pour identifier un élément dans la liste listThread, id est unique pour chaque instance de la classe ThreadNode
        // id est utilisé pour supprimer un élément se trouve dans la liste UI ListView losque l'utilisatuer clique la croix rouge
        public int id;

        public ThreadNode(Thread thread, ThreadViewItem threadViewItem)
        {
            this.thread = thread;
            this.threadViewItem = threadViewItem;
            if (threadViewItem.Type == "ballon")
                ballonCount += 1;
            else
                premierCount += 1;
            // l'ordre d'attribution est important
            count += 1;
            this.id = count;
        }
        public int getID()
        {
            return id;
        }

        public static int getCount()
        {
            return count;
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
