using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
namespace Geometry
{


    public class Graph
    {
        AABB[] navigationAABB;
        // List<Vector3Int> structureMesh;
        Dictionary<Vector3Int, int> structureMesh = new Dictionary<Vector3Int, int>();


        int[][] nodesToneighbours;

        public Graph(AABB[] NavigationAABB)
        {
            this.navigationAABB = NavigationAABB;
            //this.structureMesh = navigationAABB.Select<AABB, Vector3Int>(box => box.structure).ToList();
            structureMesh.EnsureCapacity(navigationAABB.Length);
            for (int i = 0; i < navigationAABB.Length; i++) { structureMesh.Add(navigationAABB[i].structure, i); }

            nodesToneighbours = new int[navigationAABB.Length][];

            buildGraph();
        }


        public int[] getNeighbours(int node)
        {
            return nodesToneighbours[node];
        }


        public int getIndex(Vector3Int s)
        {
            //return structureMesh.IndexOf(s);
            return structureMesh[s];
        }
        void buildGraph()
        {

            var rangePartitioner = Partitioner.Create(0, navigationAABB.Length);



            Vector3Int[] sampleNeighbourStruct = { new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 1),
                new Vector3Int(-1, 0, 0),  new Vector3Int(0, 0, -1),
                new Vector3Int(1, 0, 1),  new Vector3Int(-1, 0, 1),
                new Vector3Int(1, 0, -1), new Vector3Int(-1, 0, -1) };

            // Loop over the partitions in parallel.
            Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                // Loop over each range element without a delegate invocation.
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    //List<int> neighboursOfAABB = getCandidateNeighbours(navigationAABB[i].structure);
                    List<int> neighboursOfAABB = getCandidateNeighbours(navigationAABB[i].structure, sampleNeighbourStruct);
                    nodesToneighbours[i] = neighboursOfAABB.ToArray();
                }
            });


            //Parallel.For(0, navigationAABB.Length, number =>
            //{
            //    List<int> neighboursOfAABB = getCandidateNeighbours(navigationAABB[number].structure);
            //    nodesToneighbours[number] = neighboursOfAABB.ToArray();
            //});



        }

        List<int> getCandidateNeighbours(Vector3Int thisStruct, Vector3Int[] sampleNeighbourStruct)
        {
            // ONLY NEIGHBOURS AT THE SAME HEIGHT ARE CONSIDERED FOR NOW

            int index;

            List<int> candidatesNeighbours = new List<int>();
            candidatesNeighbours.Capacity = 8;

            Vector3Int[] candidateStruct = { thisStruct +  sampleNeighbourStruct[0], thisStruct+ sampleNeighbourStruct[1],
                thisStruct+ sampleNeighbourStruct[2], thisStruct + sampleNeighbourStruct[3],
                thisStruct+ sampleNeighbourStruct[4], thisStruct + sampleNeighbourStruct[5],
                thisStruct + sampleNeighbourStruct[6], thisStruct + sampleNeighbourStruct[7] };

            foreach (Vector3Int candidate in candidateStruct)
            {
                // index = structureMesh.IndexOf(candidate);
                if (structureMesh.ContainsKey(candidate))
                {
                    index = structureMesh[candidate];
                    candidatesNeighbours.Add(index);
                }

                // if (index > 0) { candidatesNeighbours.Add(index); }
            }


            return candidatesNeighbours;
        }



        private void parallelJobSeaerch()
        {
            NativeArray<int> nativeNeighboursOfAABB1 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);
            NativeArray<int> nativeNeighboursOfAABB2 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);
            NativeArray<int> nativeNeighboursOfAABB3 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);
            NativeArray<int> nativeNeighboursOfAABB4 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);
            NativeArray<int> nativeNeighboursOfAABB5 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);
            NativeArray<int> nativeNeighboursOfAABB6 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);
            NativeArray<int> nativeNeighboursOfAABB7 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);
            NativeArray<int> nativeNeighboursOfAABB8 = new NativeArray<int>(navigationAABB.Length, Allocator.TempJob);

            NativeArray<Vector3Int> structures = new NativeArray<Vector3Int>(navigationAABB.Length, Allocator.TempJob);
            for (int i = 0; i < navigationAABB.Length; i++) { structures[i] = navigationAABB[i].structure; }

            IncrementByDeltaTimeJob jobData = new IncrementByDeltaTimeJob();
            jobData.nativeNeighboursOfAABB1 = nativeNeighboursOfAABB1;
            jobData.nativeNeighboursOfAABB2 = nativeNeighboursOfAABB2;
            jobData.nativeNeighboursOfAABB3 = nativeNeighboursOfAABB3;
            jobData.nativeNeighboursOfAABB4 = nativeNeighboursOfAABB4;
            jobData.nativeNeighboursOfAABB5 = nativeNeighboursOfAABB5;
            jobData.nativeNeighboursOfAABB6 = nativeNeighboursOfAABB6;
            jobData.nativeNeighboursOfAABB7 = nativeNeighboursOfAABB7;
            jobData.nativeNeighboursOfAABB8 = nativeNeighboursOfAABB8;
            jobData.structures = structures;

            // Schedule the job with one Execute per index in the results array and only 1 item per processing batch
            JobHandle handle = jobData.Schedule(navigationAABB.Length, 1);

            // Wait for the job to complete
            handle.Complete();

            int[] neigh1 = nativeNeighboursOfAABB1.ToArray();
            int[] neigh2 = nativeNeighboursOfAABB2.ToArray();
            int[] neigh3 = nativeNeighboursOfAABB3.ToArray();
            int[] neigh4 = nativeNeighboursOfAABB4.ToArray();
            int[] neigh5 = nativeNeighboursOfAABB5.ToArray();
            int[] neigh6 = nativeNeighboursOfAABB6.ToArray();
            int[] neigh7 = nativeNeighboursOfAABB7.ToArray();
            int[] neigh8 = nativeNeighboursOfAABB8.ToArray();

            // Free the memory allocated by the arrays
            nativeNeighboursOfAABB1.Dispose();
            nativeNeighboursOfAABB2.Dispose();
            nativeNeighboursOfAABB3.Dispose();
            nativeNeighboursOfAABB4.Dispose();
            nativeNeighboursOfAABB5.Dispose();
            nativeNeighboursOfAABB6.Dispose();
            nativeNeighboursOfAABB7.Dispose();
            nativeNeighboursOfAABB8.Dispose();
            structures.Dispose();


            for (int i = 0; i < navigationAABB.Length; i++)
            {
                List<int> neigh = new List<int>() { neigh1[i], neigh2[i], neigh3[i], neigh4[i], neigh5[i], neigh6[i], neigh7[i], neigh8[i] };
                neigh.RemoveAll(x => x == -1);
                nodesToneighbours[i] = neigh.ToArray();

            }
        }


        struct IncrementByDeltaTimeJob : IJobParallelFor
        {
            public NativeArray<int> nativeNeighboursOfAABB1;
            public NativeArray<int> nativeNeighboursOfAABB2;
            public NativeArray<int> nativeNeighboursOfAABB3;
            public NativeArray<int> nativeNeighboursOfAABB4;
            public NativeArray<int> nativeNeighboursOfAABB5;
            public NativeArray<int> nativeNeighboursOfAABB6;
            public NativeArray<int> nativeNeighboursOfAABB7;
            public NativeArray<int> nativeNeighboursOfAABB8;

            public NativeArray<Vector3Int> structures;


            public void Execute(int index)
            {

                Vector3Int thisStruct = structures[index];

                Vector3Int[] candidateStruct = { thisStruct +  new Vector3Int(1, 0, 0), thisStruct+ new Vector3Int(0, 0, 1),
                                                thisStruct+ new Vector3Int(-1, 0, 0), thisStruct + new Vector3Int(0, 0, -1),
                                                thisStruct+ new Vector3Int(1, 0, 1), thisStruct + new Vector3Int(-1, 0, 1),
                                                thisStruct +new Vector3Int(1, 0, -1), thisStruct + new Vector3Int(-1, 0, -1) };



                int indexStruct = structures.IndexOf(candidateStruct[0]);
                nativeNeighboursOfAABB1[index] = indexStruct;

                indexStruct = structures.IndexOf(candidateStruct[1]);
                nativeNeighboursOfAABB2[index] = indexStruct;

                indexStruct = structures.IndexOf(candidateStruct[2]);
                nativeNeighboursOfAABB3[index] = indexStruct;


                indexStruct = structures.IndexOf(candidateStruct[3]);
                nativeNeighboursOfAABB4[index] = indexStruct;

                indexStruct = structures.IndexOf(candidateStruct[4]);
                nativeNeighboursOfAABB5[index] = indexStruct;

                indexStruct = structures.IndexOf(candidateStruct[5]);
                nativeNeighboursOfAABB6[index] = indexStruct;

                indexStruct = structures.IndexOf(candidateStruct[6]);
                nativeNeighboursOfAABB7[index] = indexStruct;

                indexStruct = structures.IndexOf(candidateStruct[7]);
                nativeNeighboursOfAABB8[index] = indexStruct;

            }
        }





    }



}
