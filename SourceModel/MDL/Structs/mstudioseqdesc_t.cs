using UnityEngine;

namespace UnitySourceEngine
{
    public class mstudioseqdesc_t //Inherits SourceMdlSequenceDescBase which should be mstudioseqdescbase_t
    {
        public string name;
        public string activityName;
        public string keyValues;

        public int baseHeaderOffset;
        public int nameOffset;
        public int activityNameOffset;
        public int flags;
        public int activity;
        public int activityWeight;
        public int eventCount;
        public int eventOffset;
        public Vector3 bbMin;
        public Vector3 bbMax;
        public int blendCount;
        public int animIndexOffset;
        public int movementIndex;
        public int[] groupSize; //SizeOf 2
        public int[] paramIndex; //SizeOf 2
        public int[] paramStart; //SizeOf 2
        public float[] paramEnd; //SizeOf 2
        public int paramParent;
        public float fadeInTime;
        public float fadeOutTime;
        public int localEntryNodeIndex;
        public int localExitNodeIndex;
        public int nodeFlags;
        public float entryPhase;
        public float exitPhase;
        public float lastFrame;
        public int nextSeq;
        public int pose;
        public int ikRuleCount;
        public int autoLayerCount;
        public int autoLayerOffset;
        public int weightOffset;
        public int poseKeyOffset;
        public int ikLockCount;
        public int ikLockOffset;
        public int keyValueOffset;
        public int keyValueSize;
        public int cyclePoseIndex;
        public int activityModifierOffset;
        public int activityModifierCount;
        public int[] unused; //SizeOf 7
        public double[] poseKeys;
        public mstudioevent_t[] events;
        public mstudioautolayer_t[] autoLayers;
        public mstudioiklock_t[] ikLocks;
        public double[] boneWeights;
        public int weightListIndex;
        public short[] animDescIndices;
        public mstudioactivitymodifier_t[] activityModifiers;

        public bool boneWeightsAreDefault;

        public void Dispose()
        {
            groupSize = null;
            paramIndex = null;
            paramStart = null;
            paramEnd = null;
            unused = null;
            poseKeys = null;
            if (events != null)
                foreach (var anEvent in events)
                    anEvent?.Dispose();
            events = null;
            autoLayers = null;
            if (ikLocks != null)
                foreach (var ikLock in ikLocks)
                    ikLock?.Dispose();
            ikLocks = null;
            boneWeights = null;
            animDescIndices = null;
            activityModifiers = null;
        }
    }
}