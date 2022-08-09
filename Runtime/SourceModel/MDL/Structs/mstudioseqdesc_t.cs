using UnityEngine;

namespace UnitySourceEngine
{
    public struct mstudioseqdesc_t //Inherits SourceMdlSequenceDescBase which should be mstudioseqdescbase_t
    {
        public string name;
        public string activityName;
        public string keyValues;

        public int baseHeaderOffset; //4
        public int nameOffset; //4
        public int activityNameOffset; //4
        public int flags; //4
        public int activity; //4
        public int activityWeight; //4
        public int eventCount; //4
        public int eventOffset; //4
        public Vector3 bbMin; //12
        public Vector3 bbMax; //12
        public int blendCount; //4
        public int animIndexOffset; //4
        public int movementIndex; //4
        public int[] groupSize; //SizeOf 2
        public int[] paramIndex; //SizeOf 2
        public int[] paramStart; //SizeOf 2
        public float[] paramEnd; //SizeOf 2
        public int paramParent; //4
        public float fadeInTime; //4
        public float fadeOutTime; //4
        public int localEntryNodeIndex; //4
        public int localExitNodeIndex; //4
        public int nodeFlags; //4
        public float entryPhase; //4
        public float exitPhase; //4
        public float lastFrame; //4
        public int nextSeq; //4
        public int pose; //4
        public int ikRuleCount; //4
        public int autoLayerCount; //4
        public int autoLayerOffset; //4
        public int weightOffset; //4
        public int poseKeyOffset; //4
        public int ikLockCount; //4
        public int ikLockOffset; //4
        public int keyValueOffset; //4
        public int keyValueSize; //4
        public int cyclePoseIndex; //4
        public int activityModifierOffset; //4
        public int activityModifierCount; //4
        public int[] unused; //SizeOf 7
        public double[] poseKeys;
        public mstudioevent_t[] events;
        public mstudioautolayer_t[] autoLayers; //40*length
        public mstudioiklock_t[] ikLocks;
        public double[] boneWeights;
        public int weightListIndex; //4
        public short[] animDescIndices;
        public mstudioactivitymodifier_t[] activityModifiers;

        public bool boneWeightsAreDefault; //4

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)
            (
                (!string.IsNullOrEmpty(name) ? 2*name.Length : 0) +
                (!string.IsNullOrEmpty(activityName) ? 2*activityName.Length : 0) +
                (!string.IsNullOrEmpty(keyValues) ? 2*keyValues.Length : 0) +
                (groupSize != null ? 4*groupSize.Length : 0) +
                (paramIndex != null ? 4*paramIndex.Length : 0) +
                (paramStart != null ? 4*paramStart.Length : 0) +
                (paramEnd != null ? 4*paramEnd.Length : 0) +
                (unused != null ? 4*unused.Length : 0) +
                (poseKeys != null ? 8*poseKeys.Length : 0) +
                (autoLayers != null ? 40*autoLayers.Length : 0) +
                (animDescIndices != null ? 2*animDescIndices.Length : 0) +
                164
            );
            if (events != null)
                foreach(var ev in events)
                    totalBytes += ev.CountBytes();
            if (ikLocks != null)
                foreach(var ikLock in ikLocks)
                    totalBytes += ikLock.CountBytes();
            if (activityModifiers != null)
                foreach(var activityModifier in activityModifiers)
                    totalBytes += activityModifier.CountBytes();
            return totalBytes;
        }

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
                    anEvent.Dispose();
            events = null;
            autoLayers = null;
            if (ikLocks != null)
                foreach (var ikLock in ikLocks)
                    ikLock.Dispose();
            ikLocks = null;
            boneWeights = null;
            animDescIndices = null;
            activityModifiers = null;
        }
    }
}