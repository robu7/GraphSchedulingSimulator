using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GraphTest
{
    /// <summary>
    /// Manages saving and getting run time history of tasks
    /// </summary>
    class TaskExecutionEstimator
    {
        private Dictionary<string, XElement> taskElementMapping;
        private Dictionary<string, List<float>> taskSampleData;
        private const string fileName = @"TaskExecutionEstimation.xml";
        private XElement xmlExecutionSamples;

        public TaskExecutionEstimator()
        {
            taskElementMapping = new Dictionary<string, XElement>();
            taskSampleData = new Dictionary<string, List<float>>();

            /*
             * Load the xml file and create a new one if it does not exist
             */
            try {
                xmlExecutionSamples = XElement.Load(fileName);
            } catch (System.IO.FileNotFoundException) {

                XDocument doc = new XDocument(
                     new XElement("Tasks", new XElement("Task", new XElement("ID", "test"), new XElement("Samples", new XElement("Sample", 3000)))));
                doc.Save(fileName);

            } finally {
                xmlExecutionSamples = XElement.Load(fileName);
            }

            /*
             * Find all items in the build with their correlating sample data
             
            foreach (var item in itemList) {
                // Find the task in the xml file
                var xmlElement = xmlExecutionSamples.Elements("Task").Where(x => x.Element("ID").Value == item.Id).First();

                // Insert it to the dictionaries and save the element
                taskElementMapping.Add(item.Id, xmlElement);
                taskSampleData.Add(item.Id, new List<float>());

                // Get and save all sample data to this task
                foreach (var time in xmlElement.Element("Samples").Elements()) {
                    taskSampleData[item.Id].Add(float.Parse(time.Value));
                }
            }
            */
        }

        /// <summary>
        /// Save the last 10 samples of the task execution time, remove the oldest if more
        /// </summary>
        public void SaveExecutionTime(string taskID, float executionTime)
        {
            lock (xmlExecutionSamples) {
                var tmp = taskElementMapping[taskID];
                var samples = tmp.Element("Samples").Elements();

                // If it is the first sample 
                if (samples.Count() == 0) {
                    tmp.Element("Samples").Add(new XElement("Sample", executionTime));
                }
                // Or we have reached 10 samples and need to remove one
                else if (samples.Count() >= 10) {
                    samples.First().Remove();
                    samples.Last().AddAfterSelf(new XElement("Sample", executionTime));
                } else
                    samples.Last().AddAfterSelf(new XElement("Sample", executionTime));


                xmlExecutionSamples.Save(fileName);
            }
        }

        /// <summary>
        /// Get the average execution time for a specific task
        /// </summary>
        public float GetEstimatedTime(string taskID)
        {

            if (taskElementMapping.ContainsKey(taskID)) {
                if (taskSampleData.ContainsKey(taskID))
                    if (taskSampleData[taskID].Count != 0)
                        return taskSampleData[taskID].Average();
                return 3000;
            } else {
                var lastElement = xmlExecutionSamples.LastNode ?? xmlExecutionSamples;
                var newElement = new XElement("Task", new XElement("ID", taskID), new XElement("Samples"));
                lastElement.AddAfterSelf(newElement);
                taskElementMapping.Add(taskID, newElement);
                xmlExecutionSamples.Save(fileName);
                return 3000;
            }
        }
    }
}
