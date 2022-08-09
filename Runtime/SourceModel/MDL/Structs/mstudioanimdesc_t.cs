using System.Collections.Generic;

namespace UnitySourceEngine
{
    public struct mstudioanimdesc_t
    {
        public string name;

        public int baseHeaderOffset; //4
        public int nameOffset; //4
        public float fps; //4
        public int flags; //4
        public int frameCount; //4
        public int movementCount; //4
        public int movementOffset; //4
        public int ikRuleZeroFrameOffset; //4
        public int[] unused1; //SizeOf 5
        public int animBlock; //4
        public int animOffset; //4
        public int ikRuleCount; //4
        public int ikRuleOffset; //4
        public int animblockIkRuleOffset; //4
        public int localHierarchyCount; //4
        public int localHierarchyOffset; //4
        public int sectionOffset; //4
        public int sectionFrameOffset; //4
        public int sectionFrameCount; //4
        public short spanFrameCount; //2
        public short spanCount; //2
        public int spanOffset; //4
        public float spanStallTime; //4

        public List<List<mstudioanim_t>> sectionsOfAnimations; //84*count*count
        public mstudio_frame_anim_t aniFrameAnim;
        public mstudioikrule_t[] ikRules;
        public List<mstudioanimsections_t> sections; //4*count
        public mstudiomovement_t[] movements; //56*length
        public mstudiolocalhierarchy_t[] localHierarchies;

        public bool animIsLinkedToSequence; //4
        public mstudioseqdesc_t[] linkedSequences;

        public ulong CountBytes()
        {
            int sofTotal = 0;
            if (sectionsOfAnimations != null)
                foreach(var list in sectionsOfAnimations)
                    sofTotal += list.Count;
            ulong totalBytes = (ulong)((unused1 != null ? 4*unused1.Length : 0) + (84*sofTotal) + (sections != null ? 4*sections.Count : 0) + (movements != null ? 56*movements.Length : 0) + 86) + aniFrameAnim.CountBytes();
            if (ikRules != null)
                foreach(var ikRule in ikRules)
                    totalBytes += ikRule.CountBytes();
            if (localHierarchies != null)
                foreach(var localHeirarchy in localHierarchies)
                    totalBytes += localHeirarchy.CountBytes();
            if (linkedSequences != null)
                foreach(var linkedSequence in linkedSequences)
                    totalBytes += linkedSequence.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            unused1 = null;
            if (sectionsOfAnimations != null)
                foreach (var section in sectionsOfAnimations)
                    section?.Clear();
            sectionsOfAnimations = null;
            aniFrameAnim.Dispose();
            if (ikRules != null)
                foreach (var ikRule in ikRules)
                    ikRule.Dispose();
            ikRules = null;
            sections = null;
            movements = null;
            if (localHierarchies != null)
                foreach (var localHierarchy in localHierarchies)
                    localHierarchy.Dispose();
            localHierarchies = null;
            if (linkedSequences != null)
                foreach (var linkedSequence in linkedSequences)
                    linkedSequence.Dispose();
            linkedSequences = null;
        }
    }

    public enum animdesc_flags
    {
        STUDIO_LOOPING = 0x0001, //ending frame should be the same as the starting frame
        STUDIO_SNAP = 0x0002, //do not interpolate between previous animation and this one
        STUDIO_DELTA = 0x0004, //this sequence "adds" to the base sequences, not slerp blends
        STUDIO_AUTOPLAY = 0x0008, //temporary flag that forces the sequence to always play
        STUDIO_POST = 0x0010, //
        STUDIO_ALLZEROS = 0x0020, //this animation/sequence has no real animation data
        STUDIO_CYCLEPOSE = 0x0080, //cycle index is taken from a pose parameter index
        STUDIO_REALTIME = 0x0100, //cycle inex is taken from a real-time clock, not the animations cycle index
        STUDIO_LOCAL = 0x0200, //sequence has a local context sequence
        STUDIO_HIDDEN = 0x0400, //don't show in default selection views
        STUDIO_OVERRIDE = 0x0800, //a forward declared sequence (empty)
        STUDIO_ACTIVITY = 0x1000, //Has been updated at runtime to activity index
        STUDIO_EVENT = 0x2000, //Has been updated at runtime event index
        STUDIO_WORLD = 0x4000,  //sequence blends in worldspace
        STUDIO_FRAMEANIM = 0x0040, //animation is encoded as by frame x bone instead of RLE bone x frame
        STUDIO_NOFORCELOOP = 0x8000, //do not force the animation loop
        STUDIO_EVENT_CLIENT = 0x10000, //Has been updated at runtime to event index on client
    }
}