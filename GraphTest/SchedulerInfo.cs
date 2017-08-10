using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphTest
{
    public partial class SchedulerInfo : Form
    {
        static List<Worker> workerList;
        static Timer updateTimer;
        static int[] activeControls;
        static Dictionary<int,List<ProgressBar>> allControls;
        static bool[] finishedStatus;

        public SchedulerInfo(List<Worker> workers)
        {
            workerList = workers;
            InitializeComponent();
            updateTimer = new Timer();
            activeControls = new int[workerList.Count];
            allControls = new Dictionary<int, List<ProgressBar>>();
            finishedStatus = new bool[workerList.Count];

            foreach (var worker in workerList) {
                var label = new Label();
                label.Text = "Worker" + worker.WorkerID;
                flowLayoutPanel1.Controls.Add(label);
                allControls[worker.WorkerID] = new List<ProgressBar>();
                finishedStatus[worker.WorkerID] = false;
                ProgressBar bar = new ProgressBar();

                foreach (var slot in worker.FullSchedule) {
                    if (slot is WorkSlot) {
                        var task = (slot as WorkSlot).TaskToBeExecuted;
                        bar = new ProgressBar();
                        bar.Maximum = slot.Size;
                        bar.Size = new Size((int)(0.1 * slot.Size), 23);
                    } else {
                        var task = (slot as IdleSlot);
                        bar = new ProgressBar();
                        bar.Maximum = slot.Size;
                        bar.Size = new Size((int)(0.1 * slot.Size), 23);
                        bar.ForeColor = Color.Red;
                    }

                    allControls[worker.WorkerID].Add(bar);
                    activeControls[worker.WorkerID] = 0;
                    flowLayoutPanel1.Controls.Add(bar);
                }
                flowLayoutPanel1.SetFlowBreak(bar, true);


                updateTimer.Interval = 100;
                updateTimer.Tick += new EventHandler(Update);
                updateTimer.Start();
            }            
        }

        private static void Update(Object myObject, EventArgs myEventArgs)
        {
            for (int i = 0; i < 4; i++) {
                if (finishedStatus[i])
                    continue;
                var activeControl = activeControls[i];

                if (allControls[i][activeControl].Value+50 >= allControls[i][activeControl].Maximum) {
                    allControls[i][activeControl].Value = allControls[i][activeControl].Maximum;
                    ++activeControls[i];
                    if (++activeControl >= allControls[i].Count)
                        finishedStatus[i] = true;
                }else {
                    allControls[i][activeControl].Value += 50;
                }
            }            
        }
    }
}
