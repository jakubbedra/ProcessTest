using System.Diagnostics;
using System.Management;
using ProcessTest;

public class ProcessModel
{
    public Process Parent { get; set; }
    public List<Process> Children { get; set; }

    public ProcessModel(Process parent)
    {
        Parent = parent;
        Children = new List<Process>();
    }

    public ProcessModel(List<Process> children)
    {
        Children = children;
    }
}

public class Program
{
    private static int GetParentProcess(int Id)
    {
        int parentPid = 0;
        using (ManagementObject mo = new ManagementObject("win32_process.handle='" + Id.ToString() + "'"))
        {
            mo.Get();
            parentPid = Convert.ToInt32(mo["ParentProcessId"]);
        }

        return parentPid;
    }

    public static List<Process> GetAvailableProcesses()
    {
        Process[] allProcesses = Process.GetProcesses();
        List<Process> processes = new List<Process>();
        foreach (Process process in allProcesses)
        {
            try
            {
                var processProcessName = process.ProcessName;
                processes.Add(process);
            }
            catch
            {
            }
        }

        return processes;
    }

    public static Dictionary<int, Process> GetProcesses()
    {
        Dictionary<int, Process> processes = new Dictionary<int, Process>();
        List<Process> allProcesses = GetAvailableProcesses();
        foreach (Process process in allProcesses)
        {
            processes.Add(process.Id, process);
        }

        return processes;
    }

    public static Dictionary<int, ProcessModel> GetProcessModels()
    {
        Dictionary<int, ProcessModel> processModels = new Dictionary<int, ProcessModel>();

        Dictionary<int, Process> processes = GetProcesses();

        foreach (KeyValuePair<int, Process> process in processes)
        {
            int parentProcess = GetParentProcess(process.Key);
            // if the parent is not present on the GetProcesses list, assume that this process is the parent
            if (!processes.ContainsKey(parentProcess))
            {
                if (processModels.ContainsKey(process.Key))
                {
                    processModels[process.Key].Parent = process.Value;
                }
                else
                {
                    processModels.Add(process.Key, new ProcessModel(process.Value));
                }
            }
            else
            {
                if (processModels.ContainsKey(parentProcess))
                {
                    processModels[parentProcess].Children.Add(process.Value);
                }
                else
                {
                    List<Process> children = new List<Process>();
                    children.Add(process.Value);
                    processModels.Add(parentProcess, new ProcessModel(children));
                }
            }
        }

        return processModels;
    }

    public static void WriteProcess(Process p, PerformanceCounter PC)
    {
        int memsize = 0; // memsize in KB
        //PC.CategoryName = "Process";
        //PC.CounterName = "Working Set - Private";
        //PC.InstanceName = p.ProcessName;
        //memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
        //PC.Close();
        //PC.Dispose();

        Console.WriteLine("Process: {0} \t\t\t\t ID: {1} \t\t\t\t Memory: {2}B",
            p.ProcessName, p.Id, memsize);
    }

    public static void Main(string[] args)
    {
        Process[] allProcesses = Process.GetProcesses();

        PerformanceCounter PC = new PerformanceCounter();
        //Dictionary<int,ProcessModel> processModels = GetProcessModels();
        //foreach (KeyValuePair<int, ProcessModel> process in processModels)
        //{
        //    WriteProcess(process.Value.Parent, PC);
        //    foreach (Process child in process.Value.Children)
        //    {
        //        Console.Write("--------");
        //        WriteProcess(child, PC);
        //    }
        //}

        foreach (Process p in allProcesses)
        {
            //int t1 = p.TotalProcessorTime.Milliseconds;
            //var privateMemorySize64 = Process.GetProcessById(p.Id).PrivateMemorySize64;
            // a counterami mozna calego systemu -> mozna wyliczyc procenty

            // cpu time chyba sie da wyliczyc, chyba...
            //var processById = Process.GetProcessById(p.Id);
            //int t2 = processById.TotalProcessorTime.Milliseconds;

            try
            {
                //int parent = GetParentProcess(p.Id);
                int parent = 0;
                int memsize = 0; // memsize in KB
                PC.CategoryName = "Process";
                PC.CounterName = "Working Set - Private";
                PC.InstanceName = p.ProcessName;
                memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
                int size = (int)p.WorkingSet64 / 1024;
                Console.WriteLine("Parent: {3}\t Process: {0} \t\t\t\t ID: {1} \t\t\t\t Memory: {2}B [{4}B] CPU: {5}",
                    p.ProcessName, p.Id, memsize, parent, size, p.TotalProcessorTime);
            }
            catch (Exception)
            {
                //bedzie trzeba taki try-catch zrobic i wsm jeszccze ogarnac jak zwijac procesy w liste
                //process macierzysty -> procesy potomne
                //i zrobic do tego model cos jak:
                //
                //ProcessModel{
                // Process parent;
                // List<Process> children;
                //}
                //
                //
            }
        }
        
        
    }
}