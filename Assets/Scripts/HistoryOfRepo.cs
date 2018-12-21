using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHub.Logging;
using UnityEngine;
using GitHub.Unity;
using UnityEngine.UI;

public class HistoryOfRepo : MonoBehaviour {

	public HistoryEntry panelPrefab;
	public Transform panelParent;
	public Text repositoryPath;
	private IGitClient gitClient;
	private IEnvironment environment;
	private IProcessManager processManager;
	private List<HistoryEntry> prefabsHistory = new List<HistoryEntry>();
	private List<HistoryEntry> prefabsStatus = new List<HistoryEntry>();
	
	public void LoadHistory()
	{
		SelectRepository(repositoryPath.text.ToNPath());
		Clear(prefabsHistory);
		gitClient.Log()
			.ThenInUI((success, entries) => 
			{
				foreach (var commit in entries)
				{
					prefabsHistory.Add(AddEntry(commit.Summary, commit.Description));
				}
			})
			.Catch(ex => Debug.Log(ex))
			.Start();
	}

	public void LoadStatus()
	{
		SelectRepository(repositoryPath.text.ToNPath());
		Clear(prefabsStatus);
		gitClient.Status()
			.ThenInUI((success, entries) => 
			{
				foreach (var stat in entries.Entries)
				{
					prefabsStatus.Add(AddEntry(stat.Path, stat.Status.ToString()));
				}
			})
			.Catch(ex => Debug.Log(ex))
			.Start();
	}

	private HistoryEntry AddEntry(string subject, string body)
	{
		var entry = Instantiate(panelPrefab, panelParent);
		entry.subject.text = subject;
		entry.body.text = body;
		return entry;
	}

	private void Clear(List<HistoryEntry> list)
	{
		foreach (var l in list)
		{
			Object.Destroy(l.gameObject);
		}
		list.Clear();
	}

	// Use this for initialization
	void Start()
	{

		LogHelper.LogAdapter = new UnityLogAdapter();
		environment = new DefaultEnvironment(new CacheContainer());
		environment.Initialize("", Application.dataPath.ToNPath(), NPath.Default, NPath.Default, Application.dataPath.ToNPath());
		var taskManager = new TaskManager(TaskScheduler.FromCurrentSynchronizationContext());
		var gitEnvironment = new ProcessEnvironment(environment);
		processManager = new ProcessManager(environment, gitEnvironment, taskManager.Token);		
		gitClient = new GitClient(environment, processManager, TaskManager.Instance.Token);
	}

	public void SelectRepository(NPath path)
	{
		environment.InitializeRepository(path);		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}