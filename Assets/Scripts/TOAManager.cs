using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;

public class TOAManager
{
    public static float[,,] TOA;
    public static int debug_file_number = 0; // Used to name the files that we output to read the TOA
    public static string lstoa_file = TerrainManager.chid + "_lstoa.sf";
    public static List<float> time_list;

    //PRE: Global TOA must be set and int slice must be within the bounds of the TOA
    //     snapshots or st = -1
    //POST: returns 2D array of TOA at time slice st (if st = -1, returns the last index)
    public static double[,] toa_snapshot(float[,,] TOA, int slice)
    {
        int len = TOA.GetLength(0);
        int width = TOA.GetLength(1);
        double[,] toa_snap = new double[len, width];
        if (slice == -1)
        {
            slice = time_list.Count;
        }
        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < width; j++)
            {
                toa_snap[i, j] = TOA[i, j, slice];
            }
        }
        return toa_snap;
    }

    //based on slread.m by Dr. Simo Hostikka
    //PRE: TerrainGenerator must have run
    //POST: Returns a 3D array representing the toa data.
    //      The first two dimensions are the x and y coordinates, the value is the
    //      toa in seconds of that grid square. -1 indicates the fire does not reach
    //      that square. The third dimension may correspond to the slice. Russ may be
    //      able to clarify.
    public static float[,,] ReadTOA()
    {
        UnityEngine.Debug.Log("Begin ReadTOA()");

        //read the file
        FileInfo file = new DirectoryInfo(Application.persistentDataPath).GetFiles(lstoa_file).FirstOrDefault();
        BinaryReader binReader = new BinaryReader(File.Open(file.FullName, FileMode.Open));

        //set data
        int Tstart = InputManager.chunk_end - (InputManager.slice_size * InputManager.slices_per_chunk);
        int Tend = InputManager.chunk_end;
        Debug.Log("Tstart: " + Tstart);
        Debug.Log("Tend: " + Tend);

        double Tstep = TerrainManager.tStep;

        binReader.ReadSingle(); //reads in blank data

        binReader.ReadChars(30); //title
        binReader.ReadSingle();
        binReader.ReadSingle();
        binReader.ReadChars(30); //subtitle

        binReader.ReadSingle();
        binReader.ReadSingle();
        binReader.ReadChars(30); //descriptor?

        binReader.ReadSingle();
        binReader.ReadSingle();

        int xmin = binReader.ReadInt32();
        int xmax = binReader.ReadInt32();
        int ymin = binReader.ReadInt32();
        int ymax = binReader.ReadInt32();
        int zmin = binReader.ReadInt32();
        int zmax = binReader.ReadInt32();

        //calculate the grid size
        int Isize = xmax - xmin + 1;
        int Jsize = ymax - ymin + 1;
        int Ksize = zmax - zmin + 1;

        binReader.ReadSingle();

        int M;
        int N;
        if (Isize == 1)
        {
            M = Jsize;
            N = Ksize;
        }
        else if (Jsize == 1)
        {
            M = Isize;
            N = Ksize;
        }
        else
        {
            M = Isize;
            N = Jsize;
        }

        //This likely corresponds to the number of slices output by wfds
        //It is necessary for formatting, but not useful for toa. Future functions
        //trim this data out
        int nrun = (int)Math.Max(1, Math.Round((Tend - Tstart) / Tstep));

        float[,,] TOA = new float[M, N, nrun];

        int slice_index = 0;

        time_list = new List<float>();
        int time = Tstart;
        //read initial data
        while (time < Tstart)
        {
            binReader.ReadSingle();
            time = binReader.ReadInt32();
            time_list.Add(time);
            binReader.ReadSingle();

            binReader.ReadSingle();
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    //this single is the actual TOA for that gridsquare
                    TOA[i, j, slice_index] = binReader.ReadSingle();
                }
            }
            binReader.ReadSingle();
        }
        //test_val was used to prevent breaking in the matlab code
        //This file uses time < Tend, though this may not be robust.
        //More testing might try to read the file size (the commented out
        //code) instead
        float test_val = 1f;
        //inFile.BaseStream.Position != inFile.BaseStream.Length)
        while (time < Tend)
        {
            slice_index++;
            binReader.ReadSingle();

            test_val = binReader.ReadSingle();

            time_list.Add((float)test_val);
            time = (int)test_val;
            binReader.ReadSingle();

            binReader.ReadSingle();
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    //the actual toa data for the file
                    TOA[i, j, slice_index] = binReader.ReadSingle();
                }
            }
            binReader.ReadSingle();
        }
        binReader.Dispose();
        binReader.Close();
        UnityEngine.Debug.Log("End ReadTOA()");
        return TOA;
    }

    // PRE: Number of timeslices as an int. Global TOA must be set
    //  POST: Dictionary<int, List<Vector2>> of TOA info
    public static Dictionary<int, List<Vector2>> usefulTOA()
    {
        Debug.Log("Begin usefulTOA()");

        TOA = ReadTOA();

        int Tstart = InputManager.chunk_end - (InputManager.slice_size * InputManager.slices_per_chunk);
        int Tend = InputManager.chunk_end;
        Debug.Log("Tstart: " + Tstart);
        Debug.Log("Tend: " + Tend);

        int spacer = InputManager.slice_size;

        //init dictionary
        //Dictionary<timeslice, List of coordinates>
        Dictionary<int, List<Vector2>> data = new Dictionary<int, List<Vector2>>();
        for (int i = Tstart; i < Tend; i += InputManager.slice_size)
        {
            data.Add(i, new List<Vector2>());
        }

        //set the initial data
        double[,] toa = toa_snapshot(TOA, -1);
        int M = toa.GetLength(0);
        int N = toa.GetLength(1);
        double curval = 0;
        int idx = 0;
        int real_idx;
        int counter = 0;
        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                curval = toa[i, j];
                if (curval > 0) //set only useful data
                {
                    idx = (int)Math.Floor(curval / spacer);
                    real_idx = spacer * idx; //index corresponding to the real time, add the coord to the appropriate slice
                    data[real_idx].Add(new Vector2(i, j));
                    counter++;
                }
            }
        }

        //Debug info for reading TOA. writer for more human readable env
        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + @"\out_" + debug_file_number + ".txt"))
        {
            debug_file_number++;
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<int, List<Vector2>> entry in data)
            {
                sw.Write(entry.Key + ": \n");
                entry.Value.ForEach(x => sb.Append("(" + x.x.ToString() + ", " + x.y.ToString() + "), \n"));
                sw.Write(sb.ToString());
                sw.Write("\n");
                sb.Clear();
            }
        }
        Debug.Log("End usefulTOA()");
        return data;
    }
















    // //PRE: Global TOA must be set and int slice must be within the bounds of the TOA
    // //     snapshots or st = -1
    // //POST: returns 2D array of TOA at time slice st (if st = -1, returns the last index)
    // public static double[,] toa_snapshot(float[,,] TOA, int slice)
    // {
    //     int len = TOA.GetLength(0);
    //     int width = TOA.GetLength(1);
    //     double[,] toa_snap = new double[len, width];
    //     if(slice == -1)
    //     {
    //         slice = time_list.Count;
    //     }
    //     for(int i = 0; i < len; i++)
    //     {
    //         for(int j = 0; j < width; j++)
    //         {
    //             toa_snap[i, j] = TOA[i, j, slice];
    //         }
    //     }
    //     return toa_snap;
    // }

    // //based on slread.m by Dr. Simo Hostikka
    // //PRE: TerrainGenerator must have run
    // //POST: The first two dimensions are the x and y coordinates, the value is the
    // //      toa in seconds of that grid square. -1 indicates the fire does not reach
    // //      that square. The third dimension may correspond to the slice. Russ may be
    // //      able to clarify.
    // public static float[,,] ReadTOA()
    // {
    //     UnityEngine.Debug.Log("Begin ReadTOA()");
    //     //read the file
    //     FileInfo file = new DirectoryInfo(Application.persistentDataPath).GetFiles(lstoa_file).FirstOrDefault();
    //     BinaryReader binReader = new BinaryReader(File.Open(file.FullName, FileMode.Open));

    //     //set data
    //     int Tstart = InputManager.chunk_end - (InputManager.slice_size * InputManager.slices_per_chunk);
    //     int Tend = InputManager.chunk_end;
    //     Debug.Log("Tstart: " + Tstart);
    //     Debug.Log("Tend: " + Tend);

    //     double Tstep = TerrainManager.Tstep;

    //     binReader.ReadSingle(); //reads in blank data

    //     binReader.ReadChars(30); //title
    //     binReader.ReadSingle();
    //     binReader.ReadSingle();
    //     binReader.ReadChars(30); //subtitle

    //     binReader.ReadSingle();
    //     binReader.ReadSingle();
    //     binReader.ReadChars(30); //descriptor?

    //     binReader.ReadSingle();
    //     binReader.ReadSingle();

    //     int xmin = binReader.ReadInt32();
    //     int xmax = binReader.ReadInt32();
    //     int ymin = binReader.ReadInt32();
    //     int ymax = binReader.ReadInt32();
    //     int zmin = binReader.ReadInt32();
    //     int zmax = binReader.ReadInt32();

    //     //calculate the grid size
    //     int Isize = xmax - xmin + 1;
    //     int Jsize = ymax - ymin + 1;
    //     int Ksize = zmax - zmin + 1;

    //     binReader.ReadSingle();

    //     int M;
    //     int N;
    //     if (Isize == 1) {
    //         M = Jsize;
    //         N = Ksize;
    //     }
    //     else if (Jsize == 1)
    //     {
    //         M = Isize;
    //         N = Ksize;
    //     }
    //     else
    //     {
    //         M = Isize;
    //         N = Jsize;
    //     }

    //     //This likely corresponds to the number of slices output by wfds
    //     //It is necessary for formatting, but not useful for toa. Future functions
    //     //trim this data out
    //     int nrun = (int)Math.Max(1, Math.Round((Tend - Tstart) / Tstep));

    //     float[,,] TOA = new float[M,N,nrun];

    //     int slice_index = 0;

    //     time_list = new List<float>();
    //     int time = Tstart; 
    //     //read initial data
    //     while(time < Tstart)
    //     {
    //         binReader.ReadSingle();
    //         time = binReader.ReadInt32();
    //         time_list.Add(time);
    //         binReader.ReadSingle();

    //         binReader.ReadSingle();
    //         for(int i = 0; i < M; i++)
    //         {
    //             for(int j = 0; j < N; j++)
    //             {
    //                 //this single is the actual TOA for that gridsquare
    //                 TOA[i, j, slice_index] = binReader.ReadSingle();
    //             }
    //         }
    //         binReader.ReadSingle();
    //     }

    //     float test_val = 1f;
    //     while (time <  Tend)
    //     {
    //         slice_index++;
    //         binReader.ReadSingle();

    //         test_val = binReader.ReadSingle();

    //         time_list.Add((float)test_val);
    //         time = (int)test_val;
    //         binReader.ReadSingle();

    //         binReader.ReadSingle();
    //         for (int i = 0; i < M; i++)
    //         {
    //             for (int j = 0; j < N; j++)
    //             {
    //                 //the actual toa data for the file
    //                 TOA[i, j, slice_index] = binReader.ReadSingle();
    //             }
    //         }
    //         binReader.ReadSingle();
    //     }
    //     binReader.Dispose();
    //     binReader.Close();
    //     UnityEngine.Debug.Log("End ReadTOA()");
    //     return TOA;
    // }

    // // PRE:
    // // POST: Dictionary<int, List<Vector3>> of TOA info
    // public static Dictionary<int, List<Vector3>> usefulTOA()
    // {
    //     Debug.Log("Begin usefulTOA()");

    //     TOA = ReadTOA();

    //     int Tstart = InputManager.chunk_end - (InputManager.slice_size * InputManager.slices_per_chunk); // 0... 60 etc
    //     int Tend = InputManager.chunk_end; // 60... 120 etc

    //     Dictionary<int, List<Vector3>> data = new Dictionary<int, List<Vector3>>();
    //     for (int i = Tstart; i < Tend; i += InputManager.slice_size)
    //     {
    //         data.Add(i, new List<Vector3>());
    //     }

    //     //set the initial data
    //     double[,] toa = toa_snapshot(TOA, -1);
    //     int M = toa.GetLength(0);
    //     int N = toa.GetLength(1);
    //     double curval = 0;
    //     int idx = 0;
    //     int real_idx;
    //     int counter = 0;
    //     for (int i = 0; i < M; i++)
    //     {
    //         for (int j = 0; j < N; j++)
    //         {
    //             curval = toa[i, j];
    //             if(curval > 0) //set only useful data
    //             {
    //                 idx = (int)Math.Floor(curval / InputManager.slice_size);
    //                 real_idx = InputManager.slice_size * idx; //index corresponding to the real time, add the coord to the appropriate slice
    //                 //data[real_idx].Add(new Vector3(i, 200, j));
    //                 Vector3 terrain_vert = TerrainManager.find_vert(new Vector2(i, j));
    //                 Vector3 corrected_vert = new Vector3(terrain_vert.x, terrain_vert.y, terrain_vert.z);
    //                 data[real_idx].Add(corrected_vert);
    //                 counter++;
    //             }
    //         }
    //     }

    //     //Debug info for reading TOA. writer for more human readable env
    //     using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + @"\out_" + debug_file_number + ".txt"))
    //     {
    //         StringBuilder sb = new StringBuilder();
    //         foreach (KeyValuePair<int, List<Vector3>> entry in data)
    //         {
    //             sw.Write(entry.Key + ": ");
    //             sw.Write("\n");
    //             entry.Value.ForEach(x => sb.Append("(" + x.x.ToString() + ", " + x.y.ToString() + "), "));
    //             sw.Write(sb.ToString());
    //             sw.Write("\n");
    //             sb.Clear();
    //         }
    //         debug_file_number++;
    //     }
    //     Debug.Log("End usefulTOA()");
    //     return data;
    // }
}
