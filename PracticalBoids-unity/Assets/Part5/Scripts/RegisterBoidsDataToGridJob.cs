using Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Part5
{
    // MEMO: Part3のRegisterBoidsDataToGridJobをほぼコピペ
    // Execute()のメソッド最初に個体の生存状態をチェックしている
    [BurstCompile]
    public struct RegisterBoidsDataToGridJob : IJobParallelFor
    {
        [WriteOnly] private NativeParallelMultiHashMap<int3, int>.ParallelWriter _gridHashMapWrite;
        [ReadOnly] private readonly NativeArray<BoidsData> _boidsDataArrayRead;
        [ReadOnly] private readonly NativeArray<bool> _aliveFlagDataArrayRead;
        [ReadOnly] private readonly float _gridScale;

        public RegisterBoidsDataToGridJob(
            NativeParallelMultiHashMap<int3, int>.ParallelWriter gridHashMapWrite,
            NativeArray<BoidsData> boidsDataArrayRead,
            NativeArray<bool> aliveFlagDataArrayRead,
            float gridScale)
        {
            _gridHashMapWrite = gridHashMapWrite;
            _boidsDataArrayRead = boidsDataArrayRead;
            _aliveFlagDataArrayRead = aliveFlagDataArrayRead;
            _gridScale = gridScale;
        }

        public void Execute(int index)
        {
            // MEMO: 個体が生存していない場合は何もしない
            if (!_aliveFlagDataArrayRead[index])
            {
                return;
            }

            var boidsDataPosition = _boidsDataArrayRead[index].Position;

            var cellIndex = MathematicsUtility.CalculateCellIndex(boidsDataPosition, _gridScale);

            _gridHashMapWrite.Add(cellIndex, index);
        }
    }
}