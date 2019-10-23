using UnityEngine;

namespace UnitySourceEngine
{
    public struct studiohdr_t
    {
        public int id; // Model format ID, such as "IDST" (0x49 0x44 0x53 0x54)
        public int version; // Format version number, such as 48 (0x30,0x00,0x00,0x00)
        public int checkSum; // this has to be the same in the phy and vtx files to load!
        public char[] name; // The internal name of the model, padding with null bytes.
                            // Typically "my_model.mdl" will have an internal name of "my_model"
        public int dataLength;  // Data size of MDL file in bytes.

        // A vector is 12 bytes, three 4-byte float-values in a row.
        public Vector3 eyeposition; // Position of player viewpoint relative to model origin
        public Vector3 illumposition; // ?? Presumably the point used for lighting when per-vertex lighting is not enabled.
        public Vector3 hull_min; // Corner of model hull box with the least X/Y/Z values
        public Vector3 hull_max; // Opposite corner of model hull box
        public Vector3 view_bbmin; // View Bounding Box Minimum Position
        public Vector3 view_bbmax; // View Bounding Box Maximum Position

        public int flags; // Binary flags in little-endian order. 
                          // ex (00000001,00000000,00000000,11000000) means flags for position 0, 30, and 31 are set. 
                          // Set model flags section for more information

        /*
         * After this point, the header contains many references to offsets
         * within the MDL file and the number of items at those offsets.
         *
         * Offsets are from the very beginning of the file.
         * 
         * Note that indexes/counts are not always paired and ordered consistently.
         */

        // mstudiobone_t
        public int bone_count;  // Number of data sections (of type mstudiobone_t)
        public int bone_offset; // Offset of first data section

        // mstudiobonecontroller_t
        public int bonecontroller_count;
        public int bonecontroller_offset;

        // mstudiohitboxset_t
        public int hitbox_count;
        public int hitbox_offset;

        // mstudioanimdesc_t
        public int localanim_count;
        public int localanim_offset;

        // mstudioseqdesc_t
        public int localseq_count;
        public int localseq_offset;

        public int activitylistversion; // ??
        public int eventsindexed;   // ??

        // VMT texture filenames
        // mstudiotexture_t
        public int texture_count;
        public int texture_offset;

        // This offset points to a series of ints.
        // Each int value, in turn, is an offset relative to the start of this header/the-file,
        // At which there is a null-terminated string.
        public int texturedir_count;
        public int texturedir_offset;

        // Each skin-family assigns a texture-id to a skin location
        public int skinreference_count;
        public int skinrfamily_count;
        public int skinreference_index;

        // mstudiobodyparts_t
        public int bodypart_count;
        public int bodypart_offset;

        // Local attachment points		
        // mstudioattachment_t
        public int attachment_count;
        public int attachment_offset;

        // Node values appear to be single bytes, while their names are null-terminated strings.
        public int localnode_count;
        public int localnode_index;
        public int localnode_name_index;

        // mstudioflexdesc_t
        public int flexdesc_count;
        public int flexdesc_index;

        // mstudioflexcontroller_t
        public int flexcontroller_count;
        public int flexcontroller_index;

        // mstudioflexrule_t
        public int flexrules_count;
        public int flexrules_index;

        // IK probably referse to inverse kinematics
        // mstudioikchain_t
        public int ikchain_count;
        public int ikchain_index;

        // Information about any "mouth" on the model for speech animation
        // More than one sounds pretty creepy.
        // mstudiomouth_t
        public int mouths_count;
        public int mouths_index;

        // mstudioposeparamdesc_t
        public int localposeparam_count;
        public int localposeparam_index;

        /*
         * For anyone trying to follow along, as of this writing,
         * the next "surfaceprop_index" value is at position 0x0134 (308)
         * from the start of the file.
         */

        // Surface property value (single null-terminated string)
        public int surfaceprop_index;

        // Unusual: In this one index comes first, then count.
        // Key-value data is a series of strings. If you can't find
        // what you're interested in, check the associated PHY file as well.
        public int keyvalue_index;
        public int keyvalue_count;

        // More inverse-kinematics
        // mstudioiklock_t
        public int iklock_count;
        public int iklock_index;


        public float mass; // Mass of object (4-bytes)
        public int contents; // ??

        // Other models can be referenced for re-used sequences and animations
        // (See also: The $includemodel QC option.)
        // mstudiomodelgroup_t
        public int includemodel_count;
        public int includemodel_index;

        public int virtualModel; // Placeholder for mutable-void*

        // mstudioanimblock_t
        public int animblocks_name_index;
        public int animblocks_count;
        public int animblocks_index;

        public int animblockModel; // Placeholder for mutable-void*

        // Points to a series of bytes?
        public int bonetablename_index;

        public int vertex_base; // Placeholder for void*
        public int offset_base; // Placeholder for void*

        // Used with $constantdirectionallight from the QC 
        // Model should have flag #13 set if enabled
        public byte directionaldotproduct;

        public byte rootLod; // Preferred rather than clamped

        // 0 means any allowed, N means Lod 0 -> (N-1)
        public byte numAllowedRootLods;

        public byte unused1; // ??
        public int unused2; // ??

        // mstudioflexcontrollerui_t
        public int flexcontrollerui_count;
        public int flexcontrollerui_index;

        public float vertAnimFixedPointScale;
        public int surfacePropLookup;

        /**
         * Offset for additional header information.
         * May be zero if not present, or also 408 if it immediately 
         * follows this studiohdr_t
         */
        // studiohdr2_t
        public int studiohdr2index;

        public int unused3; // ??

        public override string ToString()
        {
            string output = "MDLHeader1";
            output += "\n_id(" + id + ")";
            output += "\n_version(" + version + ")";
            output += "\n_checksum(" + checkSum + ")";
            output += "\n_name(" + new string(name).Replace("\0", "") + ")";
            output += "\n_datalength(" + dataLength + ")";
            output += "\n_eyeposition(" + eyeposition + ")";
            output += "\n_illumposition(" + illumposition + ")";
            output += "\n_hull_min(" + hull_min + ")";
            output += "\n_hull_max(" + hull_max + ")";
            output += "\n_view_bbmin(" + view_bbmin + ")";
            output += "\n_view_bbmax(" + view_bbmax + ")";
            output += "\n_flags(" + flags + ")";
            output += "\n_bone_count(" + bone_count + ")";
            output += "\n_bone_offset(" + bone_offset + ")";
            output += "\n_bonecontroller_count(" + bonecontroller_count + ")";
            output += "\n_bonecontroller_offset(" + bonecontroller_offset + ")";
            output += "\n_hitbox_count(" + hitbox_count + ")";
            output += "\n_hitbox_offset(" + hitbox_offset + ")";
            output += "\n_localanim_count(" + localanim_count + ")";
            output += "\n_localanim_offset(" + localanim_offset + ")";
            output += "\n_localseq_count(" + localseq_count + ")";
            output += "\n_localseq_offset(" + localseq_offset + ")";
            output += "\n_activitylistversion(" + activitylistversion + ")";
            output += "\n_eventsindexed(" + eventsindexed + ")";
            output += "\n_texture_count(" + texture_count + ")";
            output += "\n_texture_offset(" + texture_offset + ")";
            output += "\n_texturedir_count(" + texturedir_count + ")";
            output += "\n_texturedir_offset(" + texturedir_offset + ")";
            output += "\n_skinreference_count(" + skinreference_count + ")";
            output += "\n_skinrfamily_count(" + skinrfamily_count + ")";
            output += "\n_skinreference_index(" + skinreference_index + ")";
            output += "\n_bodypart_count(" + bodypart_count + ")";
            output += "\n_bodypart_offset(" + bodypart_offset + ")";
            output += "\n_attachment_count(" + attachment_count + ")";
            output += "\n_attachment_offset(" + attachment_offset + ")";
            output += "\n_localnode_count(" + localnode_count + ")";
            output += "\n_localnode_index(" + localnode_index + ")";
            output += "\n_localnode_name_index(" + localnode_name_index + ")";
            output += "\n_flexdesc_count(" + flexdesc_count + ")";
            output += "\n_flexdesc_index(" + flexdesc_index + ")";
            output += "\n_flexcontroller_count(" + flexcontroller_count + ")";
            output += "\n_flexcontroller_index(" + flexcontroller_index + ")";
            output += "\n_flexrules_count(" + flexrules_count + ")";
            output += "\n_flexrules_index(" + flexrules_index + ")";
            output += "\n_ikchain_count(" + ikchain_count + ")";
            output += "\n_ikchain_index(" + ikchain_index + ")";
            output += "\n_mouths_count(" + mouths_count + ")";
            output += "\n_mouths_index(" + mouths_index + ")";
            output += "\n_localposeparam_count(" + localposeparam_count + ")";
            output += "\n_localposeparam_index(" + localposeparam_index + ")";
            output += "\n_surfaceprop_index(" + surfaceprop_index + ")";
            output += "\n_keyvalue_index(" + keyvalue_index + ")";
            output += "\n_keyvalue_count(" + keyvalue_count + ")";
            output += "\n_iklock_count(" + iklock_count + ")";
            output += "\n_iklock_index(" + iklock_index + ")";
            output += "\n_mass(" + mass + ")";
            output += "\n_contents(" + contents + ")";
            output += "\n_includemodel_count(" + includemodel_count + ")";
            output += "\n_includemodel_index(" + includemodel_index + ")";
            output += "\n_virtualModel(" + virtualModel + ")";
            output += "\n_animblocks_name_index(" + animblocks_name_index + ")";
            output += "\n_animblocks_count(" + animblocks_count + ")";
            output += "\n_animblocks_index(" + animblocks_index + ")";
            output += "\n_animblockModel(" + animblockModel + ")";
            output += "\n_bonetablename_index(" + bonetablename_index + ")";
            output += "\n_vertex_base(" + vertex_base + ")";
            output += "\n_offset_base(" + offset_base + ")";
            output += "\n_directionaldotproduct(" + directionaldotproduct + ")";
            output += "\n_rootLod(" + rootLod + ")";
            output += "\n_numAllowedRootLods(" + numAllowedRootLods + ")";
            output += "\n_unused1(" + unused1 + ")";
            output += "\n_unused2(" + unused2 + ")";
            output += "\n_flexcontrollerui_count(" + flexcontrollerui_count + ")";
            output += "\n_flexcontrollerui_index(" + flexcontrollerui_index + ")";
            output += "\n_vertAnimFixedPointScale(" + vertAnimFixedPointScale + ")";
            output += "\n_surfacePropLookup(" + surfacePropLookup + ")";
            output += "\n_studiohdr2index(" + studiohdr2index + ")";
            output += "\n_unused3(" + unused3 + ")";

            return output;
        }

        /**
         * As of this writing, the header is 408 bytes long in total
         */
    }
}