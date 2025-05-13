using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Part3
{
    [BurstCompile]
    public struct RegisterBoidsIndexToGridJob : IJobParallelFor
    {
        [WriteOnly] private NativeParallelMultiHashMap<int3, int>
            .ParallelWriter _gridHashMapWrite;
        [ReadOnly] private readonly NativeArray<BoidsData> _boidsDataArrayRead;
        [ReadOnly] private readonly float _cellScale;
        
        public RegisterBoidsIndexToGridJob(
            NativeParallelMultiHashMap<int3, int>.ParallelWriter gridHashMapWrite,
            NativeArray<BoidsData> boidsDataArrayRead,
            float cellScale)
        {
            _gridHashMapWrite = gridHashMapWrite;
            _boidsDataArrayRead = boidsDataArrayRead;
            _cellScale = cellScale;
        }

        public void Execute(int index)
        {
            var boidsDataPosition = _boidsDataArrayRead[index].Position;

            var cellIndex = MathematicsUtility.CalculateCellIndex(boidsDataPosition, _cellScale);

            _gridHashMapWrite.Add(cellIndex, index);
        }
    }
}