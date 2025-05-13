#if UNITY_EDITOR
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Common;

namespace Part5.Tests
{
    // MEMO: 生成AI Claudeが作成したテストコード
    public class CollisionDetectionJobTests
    {
        private NativeArray<BulletData> _bulletDatas;
        private NativeArray<BoidsData> _boidsDatas;
        private NativeParallelMultiHashMap<int3, int> _gridHashMap;
        private NativeArray<bool> _aliveFlagDatas;
        private NativeArray<CollisionData> _collisionDatas;

        private const float GRID_SCALE = 1.0f;
        private const float BOIDS_RADIUS = 0.5f;
        private const float BULLET_RADIUS = 0.25f;
        private const float DELTA_TIME = 1f;

        [SetUp]
        public void SetUp()
        {
            // テスト用のデータ構造を初期化
            _bulletDatas = new NativeArray<BulletData>(1, Allocator.TempJob);
            _boidsDatas = new NativeArray<BoidsData>(10, Allocator.TempJob);
            _gridHashMap = new NativeParallelMultiHashMap<int3, int>(10, Allocator.TempJob);
            _aliveFlagDatas = new NativeArray<bool>(10, Allocator.TempJob);
            _collisionDatas = new NativeArray<CollisionData>(5, Allocator.TempJob);

            // 全ての個体を生存状態に設定
            for (int i = 0; i < _aliveFlagDatas.Length; i++)
            {
                _aliveFlagDatas[i] = false;
            }

            for (int i = 0; i < _boidsDatas.Length; i++)
            {
                _boidsDatas[i] = new BoidsData
                {
                    Position = new float3(float.MaxValue, float.MaxValue, float.MaxValue),
                    Velocity = new float3(0, 0, 0)
                };
            }
        }

        [TearDown]
        public void TearDown()
        {
            // テスト後にNativeArrayをクリーンアップ
            _bulletDatas.Dispose();
            _boidsDatas.Dispose();
            _gridHashMap.Dispose();
            _aliveFlagDatas.Dispose();
            _collisionDatas.Dispose();
        }

        /// <summary>
        /// グリッドハッシュマップに個体を登録します
        /// </summary>
        private void RegisterBoidsToGrid()
        {
            _gridHashMap.Clear();
            for (int i = 0; i < _boidsDatas.Length; i++)
            {
                if (!_aliveFlagDatas[i])
                    continue;
                
                var position = _boidsDatas[i].Position;
                var cellIndex = MathematicsUtility.CalculateCellIndex(position, GRID_SCALE);
                _gridHashMap.Add(cellIndex, i);
            }
        }

        /// <summary>
        /// ジョブを実行して結果を待ちます
        /// </summary>
        private void ExecuteJob()
        {
            var job = new CollisionDetectionJob(
                _bulletDatas,
                _boidsDatas,
                _gridHashMap,
                GRID_SCALE,
                BOIDS_RADIUS,
                DELTA_TIME,
                _aliveFlagDatas,
                _collisionDatas
            );
            
            // var handle = job.Schedule(_bulletDatas.Length, 1);
            // handle.Complete();
            
            job.Execute(0);
        }

        [Test]
        public void Test_BulletStartPointCollision()
        {
            // Boidsの位置を設定
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(0, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;

            // 弾の位置をBoidsと重なるように設定（始点で衝突）
            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, BOIDS_RADIUS + BULLET_RADIUS - 0.1f), // 少し重なるように
                Velocity = new float3(0, 0, 10),
                Radius = BULLET_RADIUS
            };

            RegisterBoidsToGrid();
            ExecuteJob();

            // 検証
            Assert.IsTrue(_collisionDatas[0].IsCollided, "弾の始点での衝突が検出されるべき");
            Assert.IsFalse(_aliveFlagDatas[0], "衝突したBoidsは死亡状態になるべき");
        }

        [Test]
        public void Test_BulletEndPointCollision()
        {
            // Boidsの位置を設定
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(0, 0, 10),
                Velocity = new float3(0, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;

            // 弾の位置をBoidsに向かって設定（終点で衝突）
            float bulletSpeed = 10.0f;

            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(0, 0, bulletSpeed),
                Radius = BULLET_RADIUS
            };

            RegisterBoidsToGrid();
            ExecuteJob();

            // 検証
            Assert.IsTrue(_collisionDatas[0].IsCollided, "弾の終点での衝突が検出されるべき");
            Assert.IsFalse(_aliveFlagDatas[0], "衝突したBoidsは死亡状態になるべき");
        }

        [Test]
        public void Test_BulletPathCollision()
        {
            // Boidsの位置を設定（弾の移動経路上）
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(0, 0, 5),
                Velocity = new float3(0, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;

            // 弾の位置を設定
            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(0, 0, 10),
                Radius = BULLET_RADIUS
            };

            RegisterBoidsToGrid();
            ExecuteJob();

            // 検証
            Assert.IsTrue(_collisionDatas[0].IsCollided, "弾の移動経路上での衝突が検出されるべき");
            Assert.IsFalse(_aliveFlagDatas[0], "衝突したBoidsは死亡状態になるべき");

            // 衝突位置が正しいか確認（おおよそ）
            float3 expectedCollisionPos = new float3(0, 0, 5 - BOIDS_RADIUS);
            Assert.That(math.distance(_collisionDatas[0].Position, expectedCollisionPos), Is.LessThan(0.1f),
                "衝突位置がBoidsの表面上にあるべき");
        }

        [Test]
        public void Test_MultipleCollisions_SelectsFirst()
        {
            // 複数のBoidを配置（弾の移動経路上に順番に）
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(0, 0, 8),
                Velocity = new float3(0, 0, 0)
            };

            _boidsDatas[1] = new BoidsData
            {
                Position = new float3(0, 0, 5),
                Velocity = new float3(0, 0, 0)
            };

            _boidsDatas[2] = new BoidsData
            {
                Position = new float3(0, 0, 2),
                Velocity = new float3(0, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;
            _aliveFlagDatas[1] = true;
            _aliveFlagDatas[2] = true;

            // 弾の位置を設定
            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(0, 0, 10),
                Radius = BULLET_RADIUS
            };

            RegisterBoidsToGrid();
            ExecuteJob();

            // 検証 - 最初に衝突するのは一番近いBoids（インデックス2）
            Assert.IsTrue(_collisionDatas[0].IsCollided, "衝突が検出されるべき");
            Assert.IsFalse(_aliveFlagDatas[2], "最初に衝突するBoids（インデックス2）は死亡状態になるべき");
            Assert.IsTrue(_aliveFlagDatas[0], "他のBoidsは生存状態のままであるべき");
            Assert.IsTrue(_aliveFlagDatas[1], "他のBoidsは生存状態のままであるべき");

            // 衝突位置が正しいか確認（おおよそ）
            float3 expectedCollisionPos = new float3(0, 0, 2 - BOIDS_RADIUS);
            Assert.That(math.distance(_collisionDatas[0].Position, expectedCollisionPos), Is.LessThan(0.1f),
                "衝突位置が最初に衝突するBoidsの表面上にあるべき");
        }

        [Test]
        public void Test_NoCollision()
        {
            // Boidsの位置を設定（弾の移動経路から十分離れた場所）
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(5, 5, 5),
                Velocity = new float3(0, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;

            // 弾の位置を設定
            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(0, 0, 10),
                Radius = BULLET_RADIUS
            };

            RegisterBoidsToGrid();
            ExecuteJob();
            
            // 検証
            Assert.IsFalse(_collisionDatas[0].IsCollided, "衝突が検出されるべきでない");
            Assert.IsTrue(_aliveFlagDatas[0], "Boidsは生存状態のままであるべき");
        }

        [Test]
        public void Test_InactiveEntity_NoCollision()
        {
            // Boidの位置を設定
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(0, 0, 5),
                Velocity = new float3(0, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;

            // 非アクティブな弾を設定
            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(0, 0, 10),
                Radius = 0 // 半径が0は非アクティブと同じ
            };

            RegisterBoidsToGrid();
            ExecuteJob();

            // 検証
            Assert.IsFalse(_collisionDatas[0].IsCollided, "非アクティブな弾は衝突が検出されるべきでない");
            Assert.IsTrue(_aliveFlagDatas[0], "Boidsは生存状態のままであるべき");
        }

        [Test]
        public void Test_MovingBoids_Collision()
        {
            // 動くBoidsの位置を設定
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(0, 0, 5),
                Velocity = new float3(-1, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;

            // 弾の位置を設定
            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(0, 0, 10),
                Radius = BULLET_RADIUS
            };

            RegisterBoidsToGrid();
            ExecuteJob();

            // 検証 - Boidの移動は考慮されないが、衝突判定の範囲内なら検出される
            Assert.IsTrue(_collisionDatas[0].IsCollided, "弾の移動経路付近で衝突が検出されるべき");
            Assert.IsFalse(_aliveFlagDatas[0], "衝突したBoidsは死亡状態になるべき");
        }

        [Test]
        public void Test_GridBoundary_Collision()
        {
            // グリッド境界上にBoidを配置
            _boidsDatas[0] = new BoidsData
            {
                Position = new float3(GRID_SCALE, 0, 5), // X座標がグリッド境界上
                Velocity = new float3(0, 0, 0)
            };
            
            _aliveFlagDatas[0] = true;

            // 弾の位置を設定
            _bulletDatas[0] = new BulletData
            {
                Position = new float3(0, 0, 0),
                Velocity = new float3(GRID_SCALE, 0, 10), // グリッド境界を横切る
                Radius = BULLET_RADIUS
            };

            RegisterBoidsToGrid();
            ExecuteJob();

            // 検証
            Assert.IsTrue(_collisionDatas[0].IsCollided, "グリッド境界上での衝突が検出されるべき");
            Assert.IsFalse(_aliveFlagDatas[0], "衝突したBoidsは死亡状態になるべき");
        }
    }
}
#endif