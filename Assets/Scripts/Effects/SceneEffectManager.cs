using UnityEngine;

namespace ET.Client.Platform
{
    public class SceneEffectManager : MonoBehaviour, IEffectControl
    {
        public static SceneEffectManager Instance;

        [SerializeField] private ParticleSystem vfxMoveDust;

        [SerializeField] private ParticleSystem vfxJumpDust;

        [SerializeField] private ParticleSystem vfxLandDust;

        [SerializeField] private ParticleSystem vfxDashLine;

        [SerializeField] private RippleEffect vfxRippleEffect;

        [SerializeField] private GameObject vfxSpeedRing;

        [SerializeField] private TrailSnapShot trailSnapshotPrefab;

        private TrailSnapShot[] snapshots = new TrailSnapShot[64];

        public void Awake()
        {
            Instance = this;
        }

        public void Reload()
        {
            
        }
        
        public void Add(SpriteRenderer renderer, int facing, Color color, float duration = 1f,
            bool frozenUpdate = false, bool useRawDeltaTime = false)
        {
            // Vector2 scale = renderer.transform.localScale;
            // Add(renderer.transform.position,renderer.sprite,scale,facing,color,2,duration,frozenUpdate,useRawDeltaTime);
        }

        public TrailSnapShot Add(Vector2 position, Sprite sprite, Vector2 scale, int facing, Color color, int depth,
            float duration = 1f, bool frozenUpdate = false, bool useRawDeltaTime = false)
        {
            // for (int index = 0; index < this.snapshots.Length; ++index)
            // {
            //     if (this.snapshots[index] == null)
            //     {
            //         TrailSnapShot snapShot = Instantiate(trailSnapshotPrefab, this.transform);
            //         snapShot.Init(index,position,sprite,scale,color,duration,depth,frozenUpdate,useRawDeltaTime, () =>
            //         {
            //             SetSnapShot(index,null);
            //         });
            //         this.snapshots[index] = snapShot;
            //         return snapShot;
            //     }
            // }
            return null;
        }

        public void SetSnapShot(int index, TrailSnapShot snapShot)
        {
            this.snapshots[index] = snapShot;
        }

        public void ResetAllEffect()
        {
            this.vfxMoveDust.Stop();
            this.vfxMoveDust.Stop();
            this.vfxLandDust.Stop();
            this.vfxDashLine.Stop();
        }
        
        public void DashLine(Vector3 position, Vector2 dir)
        {
        }

        public void Ripple(Vector3 position)
        {
        }

        public void CameraShake(Vector2 dir)
        {
        }

        public void JumpDust(Vector2 position, Color color, Vector2 dir)
        {
        }

        public void LandDust(Vector3 position, Color color)
        {
        }

        public void SpeedRing(Vector3 position, Vector2 dir)
        {
        }

        public void Freeze(float time)
        {
        }
    }
}