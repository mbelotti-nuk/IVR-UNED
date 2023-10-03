using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.UI;
using System.Data;
using TMPro;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class FileBrowserTest : MonoBehaviour
{
    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    public Main main;
    public TMP_Dropdown dropdown;
    public TMP_Dropdown typeDropdown;
    public TMP_InputField multiplicationText;
    VoxelStructure loadMesh;
    bool rotation = false;
    bool traslate = false;
    string extension;
    Quaternion rotationMatrix;
    Vector3 traslation;
    #region rotation objects
    public TMP_InputField b1, b2, b3;
    #endregion
    #region translation objects
    public TMP_InputField delta1, delta2, delta3;
    #endregion
    public TextMeshProUGUI load;

    public void getWindow()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".jpg");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Initial filename: "Screenshot.png"
        // Title: "Save As", Submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        // FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        //						   () => { Debug.Log( "Canceled" ); },
        //						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );

        // Coroutine example
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            setDropdown();
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
            {
                Debug.Log("Start reading");
            }


            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            // byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // Or, copy the first file to persistentDataPath
            string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));

            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        }
    }


    public void setMesh()
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        float multiplier = 1; // default multiplier //main.multiplicator; 
        try
        {
            multiplier = float.Parse(multiplicationText.text, CultureInfo.InvariantCulture);
        }
        catch { }

        if (rotation)
        {
            //Debug.Log(rotation);
            if (!traslate) { traslation = new Vector3(0f, 0f, 0f); }
            main.newMesh(FileBrowser.Result[0], loadMesh, dropdown.value, dropdown.options[dropdown.value].text, typeDropdown.value, multiplier, rotationMatrix, traslation);
        }
        else
        {
            main.newMesh(FileBrowser.Result[0], loadMesh, dropdown.value, dropdown.options[dropdown.value].text, typeDropdown.value, multiplier);
        }

        Debug.Log($"Execution Time File browser: {watch.ElapsedMilliseconds * 1e-3} s");
        watch.Stop();
        load.SetText("");
        // re set rotation
        rotation = false;
        traslate = false;
    }

    void setDropdown()
    {
        dropdown.ClearOptions();

        loadMesh = new VoxelStructure();
        extension = Path.GetExtension(FileBrowser.Result[0]);
        main.readMesh(extension, FileBrowser.Result[0], ref loadMesh);
        List<string> list = loadMesh.getTallyNames();
        foreach (string n in list) { Debug.Log(n); }

        dropdown.AddOptions(list);
    }


    public void setTraslation()
    {
        // CHENGE Y WITH Z
        traslate = true;
        float x = float.Parse(delta1.text, CultureInfo.InvariantCulture);
        float y = float.Parse(delta3.text, CultureInfo.InvariantCulture);
        float z = float.Parse(delta2.text, CultureInfo.InvariantCulture);
        traslation = new Vector3(x, y, z);

    }

    public void setRotation()
    {
        rotation = true;

        // INVERT Y WITH Z AXIS 
        float alfa = float.Parse(b1.text, CultureInfo.InvariantCulture);
        float beta = float.Parse(b2.text, CultureInfo.InvariantCulture);
        float gamma = float.Parse(b3.text, CultureInfo.InvariantCulture);

        rotationMatrix = Quaternion.Euler(alfa, gamma, beta);
    }
}