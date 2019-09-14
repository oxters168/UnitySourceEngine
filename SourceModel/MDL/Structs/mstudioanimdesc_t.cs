using System.Collections.Generic;

namespace UnitySourceEngine
{
    public class mstudioanimdesc_t
    {
        public string name;

        public int baseHeaderOffset;
        public int nameOffset;
        public float fps;
        public int flags;
        public int frameCount;
        public int movementCount;
        public int movementOffset;
        public int ikRuleZeroFrameOffset;
        public int[] unused1; //SizeOf 5
        public int animBlock;
        public int animOffset;
        public int ikRuleCount;
        public int ikRuleOffset;
        public int animblockIkRuleOffset;
        public int localHierarchyCount;
        public int localHierarchyOffset;
        public int sectionOffset;
        public int sectionFrameOffset;
        public int sectionFrameCount;
        public short spanFrameCount;
        public short spanCount;
        public int spanOffset;
        public float spanStallTime;

        public List<List<mstudioanim_t>> sectionsOfAnimations;
        public mstudio_frame_anim_t aniFrameAnim;
        public mstudioikrule_t[] ikRules;
        public List<mstudioanimsections_t> sections;
        public mstudiomovement_t[] movements;
        public mstudiolocalhierarchy_t[] localHierarchies;

        public bool animIsLinkedToSequence;
        public mstudioseqdesc_t[] linkedSequences;

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
                    ikRule?.Dispose();
            ikRules = null;
            sections = null;
            movements = null;
            if (localHierarchies != null)
                foreach (var localHierarchy in localHierarchies)
                    localHierarchy?.Dispose();
            localHierarchies = null;
            if (linkedSequences != null)
                foreach (var linkedSequence in linkedSequences)
                    linkedSequence?.Dispose();
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