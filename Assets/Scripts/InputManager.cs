using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public static class InputManager
{   
    public static string CHID = "";
    public static int slice_size; // How many seconds make up a slice.
    public static int slices_per_chunk; // How many slices make up a chunk. A chunk is how long WFDS simulates in seconds. It is slice_size * slices_per_chunk.
    public static int chunk_end; // The time in seconds of when the current chunk will end
    public static int chunk_end_buffer = 5; // A buffer window after the chunk ends (we think this will help with .restart files not being written at chunk end)

    public static void writeInitialInput() 
    {
        Debug.Log("Begin Writing Initial Input");

        chunk_end = slice_size * slices_per_chunk;
        string current_line = "";
        FileInfo map = new DirectoryInfo(Application.streamingAssetsPath).GetFiles("*.fds").FirstOrDefault();
        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/input.fds")) // input.fds should match the CHID for good style. Need to change the way this function works to have the CHID before we write the file.
        {
            using (StreamReader sr = new StreamReader(map.OpenRead()))
            {
                while (!current_line.Contains("&OBST")) // Everything we need to modify is before the first &OBST line.
                {
                    current_line = sr.ReadLine(); // Read a line from the file.
                    if (current_line.Contains("&HEAD")) // &HEAD contains the CHID for the file.
                    {
                        CHID = Regex.Match(current_line, @"\'([^\}]+)\'").Value.ToString();
                    }
                    else if (current_line.Contains("&TIME T_END"))
                    {
                        current_line = "&TIME T_END= " + (chunk_end + chunk_end_buffer) + " /";
                    }
                    else if (current_line.Contains("&DUMP"))
                    {
                        current_line = current_line.Replace(" /", ","); // Remove the line ending and set a , so we can add DT_RESTART
                        current_line += ("DT_RESTART=" + slice_size.ToString() + " /"); // Output a restart file every slice_size seconds
                    }
                    else if (current_line.Contains("&MISC"))
                    {
                        current_line = current_line.Replace(" /", ","); // Remove the line ending
                    }
                    sw.WriteLine(current_line);
                }
                sw.Write(sr.ReadToEnd()); // Write the rest of the file.   
            }
        }
        FileInfo fds_file = new DirectoryInfo(Application.persistentDataPath).GetFiles("*.fds").FirstOrDefault();
        
        Debug.Log("End Writing Initial Input");
    }

    public static void writeRestartInput() {
        Debug.Log("Begin Writing Restart Input");

        chunk_end += (slice_size * slices_per_chunk); // Increase the chunk_end to be one chunk further
        Debug.Log("Next chunk_end: " + chunk_end);
        string current_line = "";
        FileInfo fds_file = new DirectoryInfo(Application.persistentDataPath).GetFiles("*.fds").FirstOrDefault();
        using (StreamReader sr = new StreamReader(fds_file.OpenRead()))
        {
            using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + @"\" + chunk_end + ".fds"))
            {
                while (!current_line.Contains("&OBST")) // Everything we need to modify is before the first &OBST line.
                {
                    current_line = sr.ReadLine();
                    if (current_line.Contains("&TIME T_END"))
                    {
                        current_line = "&TIME T_END= " + (chunk_end + chunk_end_buffer) + " /";
                    }
                    else if (current_line.Contains("&MISC") && !current_line.Contains("RESTART=.TRUE."))
                    {
                        //current_line = current_line.Replace(" /", ""); // Remove the line ending and set a , so we can add restarts
                        current_line += "RESTART=.TRUE.,";
                    }
                    sw.WriteLine(current_line);
                }
                sw.WriteLine(sr.ReadToEnd());
            }
        }
        fds_file.Delete();

        Debug.Log("End Writing Restart Input");
    }
}
