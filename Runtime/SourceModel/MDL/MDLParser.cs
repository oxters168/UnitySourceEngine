﻿using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace UnitySourceEngine
{
    public class MDLParser : IDisposable
    {
        private long fileBeginOffset;

        public MDLParser()
        {
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~MDLParser()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            // if (disposing)
            // {
            //     if (mdlData.bones != null)
            //         foreach (var bone in mdlData.bones)
            //             bone?.Dispose();
            //     // bones = null;
            //     if (mdlData.bodyParts != null)
            //         foreach (var bodyPart in mdlData.bodyParts)
            //             bodyPart?.Dispose();
            //     // bodyParts = null;
            //     if (mdlData.attachments != null)
            //         foreach (var attachment in mdlData.attachments)
            //             attachment?.Dispose();
            //     // attachments = null;
            //     if (mdlData.animDescs != null)
            //         foreach (var animDesc in mdlData.animDescs)
            //             animDesc?.Dispose();
            //     // animDescs = null;
            //     if (mdlData.textures != null)
            //         foreach (var texture in mdlData.textures)
            //             texture?.Dispose();
            //     // textures = null;
            //     // texturePaths = null;
            //     //header1?.Dispose();
            //     //header2?.Dispose();
            // }
        }

        // public void ParseHeader(Stream stream, long offsetPosition = 0)
        // {
        //     fileBeginOffset = offsetPosition;

        //     mdlData.SetHeader1(ParseHeader1(stream, offsetPosition));
        //     mdlData.SetHeader2(ParseHeader2(stream));
        // }
        public MDLData Parse(Stream stream, long offsetPosition = 0)
        {
            fileBeginOffset = offsetPosition;

            MDLData mdlData = new MDLData();
            mdlData.header1 = ParseHeader1(stream, offsetPosition);
            mdlData.header2 = ParseHeader2(stream);

            mdlData.bones = ParseBones(stream, fileBeginOffset, mdlData.header1);
            mdlData.bodyParts = ParseBodyParts(stream, fileBeginOffset, mdlData.header1);
            mdlData.textures = ParseTextures(stream, fileBeginOffset, mdlData.header1);
            mdlData.texturePaths = ParseTexturePaths(stream, fileBeginOffset, mdlData.header1);
            return mdlData;
        }
        private static studiohdr_t ParseHeader1(Stream stream, long fileBeginOffset)
        {
            var parsed = new studiohdr_t();

            stream.Position = fileBeginOffset;

            parsed.id = DataParser.ReadInt(stream); // Model format ID, such as "IDST" (0x49 0x44 0x53 0x54)
            parsed.version = DataParser.ReadInt(stream); // Format version number, such as 48 (0x30,0x00,0x00,0x00)
            parsed.checkSum = DataParser.ReadInt(stream); // this has to be the same in the phy and vtx files to load!
            char[] name = new char[64];
            for (int i = 0; i < name.Length; i++)
            {
                name[i] = DataParser.ReadChar(stream);
            }
            parsed.name = new string(name).Replace("\0", ""); // The internal name of the model, padding with null bytes.
                                 // Typically "my_model.mdl" will have an internal name of "my_model"
            // this.name = new string(name).Replace("\0", "");
            parsed.dataLength = DataParser.ReadInt(stream);    // Data size of MDL file in bytes.

            // A vector is 12 bytes, three 4-byte float-values in a row.
            parsed.eyeposition = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream)); // Position of player viewpoint relative to model origin
            parsed.illumposition = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream)); // ?? Presumably the point used for lighting when per-vertex lighting is not enabled.
            parsed.hull_min = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream)); // Corner of model hull box with the least X/Y/Z values
            parsed.hull_max = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream)); // Opposite corner of model hull box
            parsed.view_bbmin = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream)); // View Bounding Box Minimum Position
            parsed.view_bbmax = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream)); // View Bounding Box Maximum Position

            parsed.flags = DataParser.ReadInt(stream); // Binary flags in little-endian order. 
                                                        // ex (00000001,00000000,00000000,11000000) means flags for position 0, 30, and 31 are set. 
                                                        // Set model flags section for more information

            //Debug.Log("ID: " + header1.id + ", \nVersion: " + header1.version + ", \nCheckSum: " + header1.checkSum + ", \nName: " + this.name + ", \nLength: " + header1.dataLength);
            //Debug.Log("EyePos: " + header1.eyeposition + ", \nIllumPos: " + header1.illumposition + ", \nHullMin: " + header1.hull_min + ", \nHullMax: " + header1.hull_max + ", \nViewBBMin: " + header1.view_bbmin + ", \nViewBBMax: " + header1.view_bbmax);

            /*
             * After this point, the header contains many references to offsets
             * within the MDL file and the number of items at those offsets.
             *
             * Offsets are from the very beginning of the file.
             * 
             * Note that indexes/counts are not always paired and ordered consistently.
             */

            // mstudiobone_t
            parsed.bone_count = DataParser.ReadInt(stream);    // Number of data sections (of type mstudiobone_t)
            parsed.bone_offset = DataParser.ReadInt(stream);   // Offset of first data section

            // mstudiobonecontroller_t
            parsed.bonecontroller_count = DataParser.ReadInt(stream);
            parsed.bonecontroller_offset = DataParser.ReadInt(stream);

            // mstudiohitboxset_t
            parsed.hitbox_count = DataParser.ReadInt(stream);
            parsed.hitbox_offset = DataParser.ReadInt(stream);

            // mstudioanimdesc_t
            parsed.localanim_count = DataParser.ReadInt(stream);
            parsed.localanim_offset = DataParser.ReadInt(stream);

            // mstudioseqdesc_t
            parsed.localseq_count = DataParser.ReadInt(stream);
            parsed.localseq_offset = DataParser.ReadInt(stream);

            parsed.activitylistversion = DataParser.ReadInt(stream); // ??
            parsed.eventsindexed = DataParser.ReadInt(stream); // ??

            // VMT texture filenames
            // mstudiotexture_t
            parsed.texture_count = DataParser.ReadInt(stream);
            parsed.texture_offset = DataParser.ReadInt(stream);

            // This offset points to a series of ints.
            // Each int value, in turn, is an offset relative to the start of this header/the-file,
            // At which there is a null-terminated string.
            parsed.texturedir_count = DataParser.ReadInt(stream);
            parsed.texturedir_offset = DataParser.ReadInt(stream);

            // Each skin-family assigns a texture-id to a skin location
            parsed.skinreference_count = DataParser.ReadInt(stream);
            parsed.skinrfamily_count = DataParser.ReadInt(stream);
            parsed.skinreference_index = DataParser.ReadInt(stream);

            // mstudiobodyparts_t
            parsed.bodypart_count = DataParser.ReadInt(stream);
            parsed.bodypart_offset = DataParser.ReadInt(stream);

            // Local attachment points		
            // mstudioattachment_t
            parsed.attachment_count = DataParser.ReadInt(stream);
            parsed.attachment_offset = DataParser.ReadInt(stream);

            // Node values appear to be single bytes, while their names are null-terminated strings.
            parsed.localnode_count = DataParser.ReadInt(stream);
            parsed.localnode_index = DataParser.ReadInt(stream);
            parsed.localnode_name_index = DataParser.ReadInt(stream);

            // mstudioflexdesc_t
            parsed.flexdesc_count = DataParser.ReadInt(stream);
            parsed.flexdesc_index = DataParser.ReadInt(stream);

            // mstudioflexcontroller_t
            parsed.flexcontroller_count = DataParser.ReadInt(stream);
            parsed.flexcontroller_index = DataParser.ReadInt(stream);

            // mstudioflexrule_t
            parsed.flexrules_count = DataParser.ReadInt(stream);
            parsed.flexrules_index = DataParser.ReadInt(stream);

            // IK probably referse to inverse kinematics
            // mstudioikchain_t
            parsed.ikchain_count = DataParser.ReadInt(stream);
            parsed.ikchain_index = DataParser.ReadInt(stream);

            // Information about any "mouth" on the model for speech animation
            // More than one sounds pretty creepy.
            // mstudiomouth_t
            parsed.mouths_count = DataParser.ReadInt(stream);
            parsed.mouths_index = DataParser.ReadInt(stream);

            // mstudioposeparamdesc_t
            parsed.localposeparam_count = DataParser.ReadInt(stream);
            parsed.localposeparam_index = DataParser.ReadInt(stream);

            /*
             * For anyone trying to follow along, as of this writing,
             * the next "surfaceprop_index" value is at position 0x0134 (308)
             * from the start of the file.
             */
            //stream.Position = 308;

            // Surface property value (single null-terminated string)
            parsed.surfaceprop_index = DataParser.ReadInt(stream);

            // Unusual: In this one index comes first, then count.
            // Key-value data is a series of strings. If you can't find
            // what you're interested in, check the associated PHY file as well.
            parsed.keyvalue_index = DataParser.ReadInt(stream);
            parsed.keyvalue_count = DataParser.ReadInt(stream);

            // More inverse-kinematics
            // mstudioiklock_t
            parsed.iklock_count = DataParser.ReadInt(stream);
            parsed.iklock_index = DataParser.ReadInt(stream);


            parsed.mass = DataParser.ReadFloat(stream); // Mass of object (4-bytes)
            parsed.contents = DataParser.ReadInt(stream); // ??

            // Other models can be referenced for re-used sequences and animations
            // (See also: The $includemodel QC option.)
            // mstudiomodelgroup_t
            parsed.includemodel_count = DataParser.ReadInt(stream);
            parsed.includemodel_index = DataParser.ReadInt(stream);

            parsed.virtualModel = DataParser.ReadInt(stream); // Placeholder for mutable-void*

            // mstudioanimblock_t
            parsed.animblocks_name_index = DataParser.ReadInt(stream);
            parsed.animblocks_count = DataParser.ReadInt(stream);
            parsed.animblocks_index = DataParser.ReadInt(stream);

            parsed.animblockModel = DataParser.ReadInt(stream); // Placeholder for mutable-void*

            // Points to a series of bytes?
            parsed.bonetablename_index = DataParser.ReadInt(stream);

            parsed.vertex_base = DataParser.ReadInt(stream); // Placeholder for void*
            parsed.offset_base = DataParser.ReadInt(stream); // Placeholder for void*

            // Used with $constantdirectionallight from the QC 
            // Model should have flag #13 set if enabled
            parsed.directionaldotproduct = DataParser.ReadByte(stream);

            parsed.rootLod = DataParser.ReadByte(stream); // Preferred rather than clamped

            // 0 means any allowed, N means Lod 0 -> (N-1)
            parsed.numAllowedRootLods = DataParser.ReadByte(stream);

            //header.unused; // ??
            parsed.unused1 = DataParser.ReadByte(stream);
            //header.unused; // ??
            parsed.unused2 = DataParser.ReadInt(stream);

            // mstudioflexcontrollerui_t
            parsed.flexcontrollerui_count = DataParser.ReadInt(stream);
            parsed.flexcontrollerui_index = DataParser.ReadInt(stream);

            parsed.vertAnimFixedPointScale = DataParser.ReadFloat(stream);
            parsed.surfacePropLookup = DataParser.ReadInt(stream);

            /**
             * Offset for additional header information.
             * May be zero if not present, or also 408 if it immediately 
             * follows this studiohdr_t
             */
            // studiohdr2_t
            parsed.studiohdr2index = DataParser.ReadInt(stream);

            //header.unused; // ??
            parsed.unused3 = DataParser.ReadInt(stream);

            return parsed;
        }
        private static studiohdr2_t ParseHeader2(Stream stream)
        {
            var parsed = new studiohdr2_t();

            parsed.srcbonetransform_count = DataParser.ReadInt(stream);
            parsed.srcbonetransform_index = DataParser.ReadInt(stream);
            parsed.illumpositionattachmentindex = DataParser.ReadInt(stream);
            parsed.flMaxEyeDeflection = DataParser.ReadFloat(stream);
            parsed.linearbone_index = DataParser.ReadInt(stream);

            parsed.sznameindex = DataParser.ReadInt(stream);
            parsed.m_nBoneFlexDriverCount = DataParser.ReadInt(stream);
            parsed.m_nBoneFlexDriverIndex = DataParser.ReadInt(stream);

            int[] reserved = new int[56];
            for (int i = 0; i < reserved.Length; i++)
            {
                reserved[i] = DataParser.ReadInt(stream);
            }
            parsed.reserved = reserved;

            return parsed;
        }

        private static mstudiobone_t[] ParseBones(Stream stream, long fileBeginOffset, studiohdr_t header1)
        {
            mstudiobone_t[] bones = null;
            if (header1.bone_count >= 0)
            {
                long savePosition = fileBeginOffset + header1.bone_offset;

                bones = new mstudiobone_t[header1.bone_count];
                for (int i = 0; i < bones.Length; i++)
                {
                    stream.Position = savePosition;
                    long bonePosition = savePosition;

                    bones[i] = new mstudiobone_t();

                    bones[i].nameOffset = DataParser.ReadInt(stream);
                    bones[i].parentBoneIndex = DataParser.ReadInt(stream);
                    //stream.Position += 150;
                    bones[i].boneControllerIndex = new int[6];
                    for (int j = 0; j < bones[i].boneControllerIndex.Length; j++)
                    {
                        bones[i].boneControllerIndex[j] = DataParser.ReadInt(stream);
                    }
                    //FileReader.readInt(stream);
                    bones[i].position = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    bones[i].quat = new Quaternion(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    if (header1.version != 2531)
                    {
                        bones[i].rotation = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                        bones[i].positionScale = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                        bones[i].rotationScale = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    }
                    //FileReader.readInt(stream);
                    float[] columnExes = new float[4] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };
                    float[] columnWise = new float[4] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };
                    float[] columnZees = new float[4] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };

                    bones[i].poseToBoneColumn0 = new Vector3(columnExes[0], columnWise[0], columnZees[0]);
                    bones[i].poseToBoneColumn1 = new Vector3(columnExes[1], columnWise[1], columnZees[1]);
                    bones[i].poseToBoneColumn2 = new Vector3(columnExes[2], columnWise[2], columnZees[2]);
                    bones[i].poseToBoneColumn3 = new Vector3(columnExes[3], columnWise[3], columnZees[3]);

                    if (header1.version != 2531) bones[i].qAlignment = new Quaternion(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));

                    bones[i].flags = DataParser.ReadInt(stream);

                    bones[i].proceduralRuleType = DataParser.ReadInt(stream);
                    bones[i].proceduralRuleOffset = DataParser.ReadInt(stream);
                    bones[i].physicsBoneIndex = DataParser.ReadInt(stream);
                    bones[i].surfacePropNameOffset = DataParser.ReadInt(stream);
                    bones[i].contents = DataParser.ReadInt(stream);

                    if (header1.version != 2531)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            DataParser.ReadInt(stream);
                        }
                    }

                    savePosition = stream.Position;

                    if (bones[i].nameOffset != 0)
                    {
                        stream.Position = bonePosition + bones[i].nameOffset;
                        bones[i].name = DataParser.ReadNullTerminatedString(stream);
                    }
                    else bones[i].name = "";

                    if (bones[i].surfacePropNameOffset != 0)
                    {
                        stream.Position = bonePosition + bones[i].surfacePropNameOffset;
                        bones[i].theSurfacePropName = DataParser.ReadNullTerminatedString(stream);
                    }
                    else bones[i].theSurfacePropName = "";
                }
            }

            return bones;
        }

        private static mstudiobodyparts_t[] ParseBodyParts(Stream stream, long fileBeginOffset, studiohdr_t header1)
        {
            mstudiobodyparts_t[] bodyParts = null;
            if (header1.bodypart_count >= 0)
            {
                long nextBodyPartPosition = fileBeginOffset + header1.bodypart_offset;

                bodyParts = new mstudiobodyparts_t[header1.bodypart_count];
                for (int i = 0; i < bodyParts.Length; i++)
                {
                    stream.Position = nextBodyPartPosition;
                    long bodyPartPosition = nextBodyPartPosition;

                    bodyParts[i] = new mstudiobodyparts_t();

                    bodyParts[i].nameOffset = DataParser.ReadInt(stream);
                    bodyParts[i].modelCount = DataParser.ReadInt(stream);
                    bodyParts[i].theBase = DataParser.ReadInt(stream);
                    bodyParts[i].modelOffset = DataParser.ReadInt(stream);

                    nextBodyPartPosition = stream.Position;

                    if (bodyParts[i].nameOffset != 0)
                    {
                        stream.Position = bodyPartPosition + bodyParts[i].nameOffset;
                        bodyParts[i].name = DataParser.ReadNullTerminatedString(stream);
                    }
                    else bodyParts[i].name = "";

                    bodyParts[i].models = ParseModels(stream, bodyPartPosition, bodyParts[i].modelOffset, bodyParts[i].modelCount);
                }
            }

            return bodyParts;
        }
        private static mstudiomodel_t[] ParseModels(Stream stream, long bodyPartPosition, int modelOffset, int modelCount)
        {
            mstudiomodel_t[] models = null;
            if (modelCount >= 0)
            {
                long nextModelPosition = bodyPartPosition + modelOffset;
                models = new mstudiomodel_t[modelCount];
                for (int i = 0; i < models.Length; i++)
                {
                    stream.Position = nextModelPosition;
                    long modelPosition = nextModelPosition;

                    models[i] = new mstudiomodel_t();

                    var name = new char[64];
                    for (int j = 0; j < name.Length; j++)
                        name[j] = DataParser.ReadChar(stream);
                    models[i].name = new string(name).Replace("\0", "");
                    models[i].type = DataParser.ReadInt(stream);
                    models[i].boundingRadius = DataParser.ReadFloat(stream);
                    models[i].meshCount = DataParser.ReadInt(stream);
                    models[i].meshOffset = DataParser.ReadInt(stream);
                    models[i].vertexCount = DataParser.ReadInt(stream);
                    models[i].vertexOffset = DataParser.ReadInt(stream);
                    models[i].tangentOffset = DataParser.ReadInt(stream);
                    models[i].attachmentCount = DataParser.ReadInt(stream);
                    models[i].attachmentOffset = DataParser.ReadInt(stream);
                    models[i].eyeballCount = DataParser.ReadInt(stream);
                    models[i].eyeballOffset = DataParser.ReadInt(stream);

                    models[i].vertexData = new mstudio_modelvertexdata_t();
                    models[i].vertexData.vertexDataP = DataParser.ReadInt(stream);
                    models[i].vertexData.tangentDataP = DataParser.ReadInt(stream);

                    models[i].unused = new int[8];
                    for (int j = 0; j < models[i].unused.Length; j++)
                    {
                        models[i].unused[j] = DataParser.ReadInt(stream);
                    }

                    nextModelPosition = stream.Position;

                    models[i].eyeballs = ParseEyeballs(stream, modelPosition, models[i].eyeballOffset, models[i].eyeballCount);
                    models[i].meshes = ParseMeshes(stream, modelPosition, models[i].meshOffset, models[i].meshCount, models[i].eyeballs);
                }
            }
            return models;
        }
        private static mstudioeyeball_t[] ParseEyeballs(Stream stream, long modelPosition, int eyeballOffset, int eyeballCount)
        {
            mstudioeyeball_t[] eyeballs = null;
            if (eyeballCount >= 0 && eyeballOffset != 0)
            {
                eyeballs = new mstudioeyeball_t[eyeballCount];

                long nextEyeballPosition = modelPosition + eyeballOffset;
                for (int i = 0; i < eyeballs.Length; i++)
                {
                    stream.Position = nextEyeballPosition;
                    long eyeballPosition = nextEyeballPosition;

                    eyeballs[i] = new mstudioeyeball_t();

                    eyeballs[i].nameOffset = DataParser.ReadInt(stream);
                    eyeballs[i].boneIndex = DataParser.ReadInt(stream);
                    eyeballs[i].org = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    eyeballs[i].zOffset = DataParser.ReadFloat(stream);
                    eyeballs[i].radius = DataParser.ReadFloat(stream);
                    eyeballs[i].up = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    eyeballs[i].forward = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    eyeballs[i].texture = DataParser.ReadInt(stream);

                    eyeballs[i].unused1 = DataParser.ReadInt(stream);
                    eyeballs[i].irisScale = DataParser.ReadFloat(stream);
                    eyeballs[i].unused2 = DataParser.ReadInt(stream);

                    eyeballs[i].upperFlexDesc = new int[3];
                    eyeballs[i].lowerFlexDesc = new int[3];
                    eyeballs[i].upperTarget = new double[3];
                    eyeballs[i].lowerTarget = new double[3];

                    eyeballs[i].upperFlexDesc[0] = DataParser.ReadInt(stream);
                    eyeballs[i].upperFlexDesc[1] = DataParser.ReadInt(stream);
                    eyeballs[i].upperFlexDesc[2] = DataParser.ReadInt(stream);
                    eyeballs[i].lowerFlexDesc[0] = DataParser.ReadInt(stream);
                    eyeballs[i].lowerFlexDesc[1] = DataParser.ReadInt(stream);
                    eyeballs[i].lowerFlexDesc[2] = DataParser.ReadInt(stream);
                    eyeballs[i].upperTarget[0] = DataParser.ReadFloat(stream);
                    eyeballs[i].upperTarget[1] = DataParser.ReadFloat(stream);
                    eyeballs[i].upperTarget[2] = DataParser.ReadFloat(stream);
                    eyeballs[i].lowerTarget[0] = DataParser.ReadFloat(stream);
                    eyeballs[i].lowerTarget[1] = DataParser.ReadFloat(stream);
                    eyeballs[i].lowerTarget[2] = DataParser.ReadFloat(stream);

                    eyeballs[i].upperLidFlexDesc = DataParser.ReadInt(stream);
                    eyeballs[i].lowerLidFlexDesc = DataParser.ReadInt(stream);

                    eyeballs[i].unused = new int[4];
                    for (int j = 0; j < eyeballs[i].unused.Length; j++)
                    {
                        eyeballs[i].unused[j] = DataParser.ReadInt(stream);
                    }

                    eyeballs[i].eyeballIsNonFacs = DataParser.ReadByte(stream);

                    eyeballs[i].unused3 = new char[3];
                    for (int j = 0; j < eyeballs[i].unused3.Length; j++)
                    {
                        eyeballs[i].unused3[j] = DataParser.ReadChar(stream);
                    }
                    eyeballs[i].unused4 = new int[7];
                    for (int j = 0; j < eyeballs[i].unused4.Length; j++)
                    {
                        eyeballs[i].unused4[j] = DataParser.ReadInt(stream);
                    }

                    //Set the default value to -1 to distinguish it from value assigned to it by ReadMeshes()
                    eyeballs[i].theTextureIndex = -1;

                    nextEyeballPosition = stream.Position;

                    if (eyeballs[i].nameOffset != 0)
                    {
                        stream.Position = eyeballPosition + eyeballs[i].nameOffset;

                        eyeballs[i].name = DataParser.ReadNullTerminatedString(stream);
                    }
                    else eyeballs[i].name = "";
                }
            }

            return eyeballs;
        }
        private static mstudiomesh_t[] ParseMeshes(Stream stream, long modelPosition, int meshOffset, int meshCount, mstudioeyeball_t[] eyeballs)
        {
            mstudiomesh_t[] meshes = null;
            if (meshCount >= 0)
            {
                long nextMeshPosition = modelPosition + meshOffset;
                meshes = new mstudiomesh_t[meshCount];

                for (int i = 0; i < meshes.Length; i++)
                {
                    stream.Position = nextMeshPosition;
                    long meshPosition = nextMeshPosition;

                    meshes[i] = new mstudiomesh_t();

                    meshes[i].materialIndex = DataParser.ReadInt(stream);
                    meshes[i].modelOffset = DataParser.ReadInt(stream);
                    meshes[i].vertexCount = DataParser.ReadInt(stream);
                    meshes[i].vertexIndexStart = DataParser.ReadInt(stream);
                    meshes[i].flexCount = DataParser.ReadInt(stream);
                    meshes[i].flexOffset = DataParser.ReadInt(stream);
                    meshes[i].materialType = DataParser.ReadInt(stream);
                    meshes[i].materialParam = DataParser.ReadInt(stream);
                    meshes[i].id = DataParser.ReadInt(stream);
                    meshes[i].center = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));

                    meshes[i].vertexData = new mstudio_meshvertexdata_t();
                    meshes[i].vertexData.modelVertexDataP = DataParser.ReadInt(stream);
                    meshes[i].vertexData.lodVertexCount = new int[8];
                    for (int j = 0; j < meshes[i].vertexData.lodVertexCount.Length; j++)
                    {
                        meshes[i].vertexData.lodVertexCount[j] = DataParser.ReadInt(stream);
                    }

                    meshes[i].unused = new int[8];
                    for (int j = 0; j < meshes[i].unused.Length; j++)
                    {
                        meshes[i].unused[j] = DataParser.ReadInt(stream);
                    }

                    if (meshes[i].materialType == 1)
                    {
                        eyeballs[meshes[i].materialParam].theTextureIndex = meshes[i].materialIndex;
                    }

                    nextMeshPosition = stream.Position;

                    if (meshes[i].flexCount > 0 && meshes[i].flexOffset != 0)
                    {
                        meshes[i].flexes = ParseFlexes(stream, meshPosition, meshes[i].flexOffset, meshes[i].flexCount);
                    }

                    //stream.Position = model.theMeshes[i].vertexData.modelVertexDataP + model.theMeshes[i].vertexIndexStart;
                    //model.theMeshes[i].vertices = new Vector3[model.theMeshes[i].vertexCount];
                    //for (int j = 0; j < model.theMeshes[i].vertices.Length; j++)
                    //{
                    //    model.theMeshes[i].vertices[j] = new Vector3(FileReader.readFloat(stream), FileReader.readFloat(stream), FileReader.readFloat(stream));
                    //    if (j >= 0 && j < 100) Debug.Log("Mesh " + i + ": V" + j + " " + model.theMeshes[i].vertices[j]);
                    //}
                }
            }

            return meshes;
        }
        private static mstudioflex_t[] ParseFlexes(Stream stream, long meshPosition, int flexOffset, int flexCount)
        {
            mstudioflex_t[] flexes = null;
            if (flexCount >= 0)
            {
                long nextFlexPosition = meshPosition + flexOffset;
                flexes = new mstudioflex_t[flexCount];

                for (int i = 0; i < flexes.Length; i++)
                {
                    stream.Position = nextFlexPosition;
                    long flexPosition = nextFlexPosition;

                    flexes[i] = new mstudioflex_t();

                    //TODO
                }
            }
            return flexes;
        }

        private static mstudioattachment_t[] ParseAttachments(Stream stream, long fileBeginOffset, studiohdr_t header1)
        {
            mstudioattachment_t[] attachments = null;
            if (header1.attachment_count >= 0)
            {
                long nextAttachmentPosition = fileBeginOffset + header1.attachment_offset;

                attachments = new mstudioattachment_t[header1.attachment_count];
                for (int i = 0; i < attachments.Length; i++)
                {
                    stream.Position = nextAttachmentPosition;
                    long attachmentPosition = nextAttachmentPosition;

                    if (header1.version == 10)
                    {
                        var name = new char[64];
                        for (int j = 0; j < name.Length; j++)
                            name[j] = DataParser.ReadChar(stream);
                        attachments[i].name = new string(name).Replace("\0", "");
                        attachments[i].type = DataParser.ReadInt(stream);
                        attachments[i].bone = DataParser.ReadInt(stream);

                        attachments[i].attachmentPoint = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                        attachments[i].vectors = new Vector3[3];
                        for (int j = 0; j < attachments[i].vectors.Length; j++)
                        {
                            attachments[i].vectors[j] = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                        }
                    }
                    else
                    {
                        attachments[i].nameOffset = DataParser.ReadInt(stream);
                        attachments[i].flags = DataParser.ReadInt(stream);
                        attachments[i].localBoneIndex = DataParser.ReadInt(stream);
                        attachments[i].localM11 = DataParser.ReadFloat(stream);
                        attachments[i].localM12 = DataParser.ReadFloat(stream);
                        attachments[i].localM13 = DataParser.ReadFloat(stream);
                        attachments[i].localM14 = DataParser.ReadFloat(stream);
                        attachments[i].localM21 = DataParser.ReadFloat(stream);
                        attachments[i].localM22 = DataParser.ReadFloat(stream);
                        attachments[i].localM23 = DataParser.ReadFloat(stream);
                        attachments[i].localM24 = DataParser.ReadFloat(stream);
                        attachments[i].localM31 = DataParser.ReadFloat(stream);
                        attachments[i].localM32 = DataParser.ReadFloat(stream);
                        attachments[i].localM33 = DataParser.ReadFloat(stream);
                        attachments[i].localM34 = DataParser.ReadFloat(stream);
                        attachments[i].unused = new int[8];
                        for (int j = 0; j < attachments[i].unused.Length; j++)
                        {
                            attachments[i].unused[j] = DataParser.ReadInt(stream);
                        }
                    }

                    nextAttachmentPosition = stream.Position;

                    if (attachments[i].nameOffset != 0)
                    {
                        stream.Position = attachmentPosition + attachments[i].nameOffset;
                        attachments[i].name = DataParser.ReadNullTerminatedString(stream);
                    }
                }
            }

            return attachments;
        }

        private static mstudioanimdesc_t[] ParseAnimationDescs(Stream stream, long fileBeginOffset, studiohdr_t header1)
        {
            mstudioanimdesc_t[] animDescs = null;
            if (header1.localanim_count >= 0)
            {
                long animDescFileByteSize = 0;
                long nextAnimDescPosition = fileBeginOffset + header1.localanim_offset;

                animDescs = new mstudioanimdesc_t[header1.localanim_count];
                for (int i = 0; i < animDescs.Length; i++)
                {
                    stream.Position = nextAnimDescPosition;
                    long animDescPosition = nextAnimDescPosition;

                    animDescs[i].baseHeaderOffset = DataParser.ReadInt(stream);
                    animDescs[i].nameOffset = DataParser.ReadInt(stream);
                    animDescs[i].fps = DataParser.ReadFloat(stream);
                    animDescs[i].flags = DataParser.ReadInt(stream);
                    animDescs[i].frameCount = DataParser.ReadInt(stream);
                    animDescs[i].movementCount = DataParser.ReadInt(stream);
                    animDescs[i].movementOffset = DataParser.ReadInt(stream);

                    animDescs[i].ikRuleZeroFrameOffset = DataParser.ReadInt(stream);

                    animDescs[i].unused1 = new int[5];
                    for (int j = 0; j < animDescs[i].unused1.Length; j++)
                    {
                        animDescs[i].unused1[j] = DataParser.ReadInt(stream);
                    }

                    animDescs[i].animBlock = DataParser.ReadInt(stream);
                    animDescs[i].animOffset = DataParser.ReadInt(stream);
                    animDescs[i].ikRuleCount = DataParser.ReadInt(stream);
                    animDescs[i].ikRuleOffset = DataParser.ReadInt(stream);
                    animDescs[i].animblockIkRuleOffset = DataParser.ReadInt(stream);
                    animDescs[i].localHierarchyCount = DataParser.ReadInt(stream);
                    animDescs[i].localHierarchyOffset = DataParser.ReadInt(stream);
                    animDescs[i].sectionOffset = DataParser.ReadInt(stream);
                    animDescs[i].sectionFrameCount = DataParser.ReadInt(stream);

                    animDescs[i].spanFrameCount = DataParser.ReadShort(stream);
                    animDescs[i].spanCount = DataParser.ReadShort(stream);
                    animDescs[i].spanOffset = DataParser.ReadInt(stream);
                    animDescs[i].spanStallTime = DataParser.ReadFloat(stream);

                    nextAnimDescPosition = stream.Position;
                    if (i == 0)
                        animDescFileByteSize = nextAnimDescPosition - animDescPosition;

                    if (animDescs[i].nameOffset != 0)
                    {
                        stream.Position = animDescPosition + animDescs[i].nameOffset;
                        animDescs[i].name = DataParser.ReadNullTerminatedString(stream);
                    }
                    else
                        animDescs[i].name = "";
                }

                for (int i = 0; i < animDescs.Length; i++)
                {
                    long animDescPosition = fileBeginOffset + header1.localanim_offset + (i * animDescFileByteSize);
                    stream.Position = animDescPosition;

                    if ((((animdesc_flags)animDescs[i].flags) & animdesc_flags.STUDIO_ALLZEROS) == 0)
                    {
                        animDescs[i].sectionsOfAnimations = new List<List<mstudioanim_t>>();
                        //List<mstudioanim_t> animationSection = new List<mstudioanim_t>();
                        //animDescs[i].sectionsOfAnimations.Add(animationSection);
                        animDescs[i].sectionsOfAnimations.Add(new List<mstudioanim_t>());

                        if ((((animdesc_flags)animDescs[i].flags) & animdesc_flags.STUDIO_FRAMEANIM) != 0)
                        {
                            //if (animDescs[i].sectionOffset != 0 && animDescs[i].sectionFrameCount > 0) ;
                            //else if (animDescs[i].animBlock == 0) ;
                        }
                        else
                        {
                            if (animDescs[i].sectionOffset != 0 && animDescs[i].sectionFrameCount > 0)
                            {
                                int sectionCount = (animDescs[i].frameCount / animDescs[i].sectionFrameCount) + 2;

                                for (int j = 1; j < sectionCount; j++)
                                {
                                    animDescs[i].sectionsOfAnimations.Add(new List<mstudioanim_t>());
                                }

                                animDescs[i].sections = new List<mstudioanimsections_t>();
                                for (int j = 0; j < sectionCount; j++)
                                {
                                    ParseMdlAnimationSection(stream, animDescPosition + animDescs[i].sectionOffset, animDescs[i]);
                                }

                                if (animDescs[i].animBlock == 0)
                                {
                                    for (int j = 0; j < sectionCount; j++)
                                    {
                                        int sectionFrameCount = 0;
                                        if (j < sectionCount - 2)
                                        {
                                            sectionFrameCount = animDescs[i].sectionFrameCount;
                                        }
                                        else
                                        {
                                            sectionFrameCount = animDescs[i].frameCount - ((sectionCount - 2) * animDescs[i].sectionFrameCount);
                                        }

                                        ParseMdlAnimation(animDescPosition + animDescs[i].sections[j].animOffset, animDescs[i], sectionFrameCount, animDescs[i].sectionsOfAnimations[j]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return animDescs;
        }
        private static void ParseMdlAnimationSection(Stream stream, long sectionPosition, mstudioanimdesc_t animDesc)
        {
            stream.Position = sectionPosition;

            mstudioanimsections_t animSection = new mstudioanimsections_t();
            animSection.animBlock = DataParser.ReadInt(stream);
            animSection.animOffset = DataParser.ReadInt(stream);
            animDesc.sections.Add(animSection);
        }
        private static void ParseMdlAnimation(long animPosition, mstudioanimdesc_t animDesc, int sectionFrameCount, List<mstudioanim_t> sectionOfAnims)
        {

        }

        private static mstudiotexture_t[] ParseTextures(Stream stream, long fileBeginOffset, studiohdr_t header1)
        {
            mstudiotexture_t[] textures = null;
            if (header1.texture_count >= 0)
            {
                long nextTexturePosition = fileBeginOffset + header1.texture_offset;

                textures = new mstudiotexture_t[header1.texture_count];
                for (int i = 0; i < textures.Length; i++)
                {
                    stream.Position = nextTexturePosition;
                    long texturePosition = nextTexturePosition;

                    textures[i] = new mstudiotexture_t();

                    textures[i].nameOffset = DataParser.ReadInt(stream);
                    textures[i].flags = DataParser.ReadInt(stream);
                    textures[i].used = DataParser.ReadInt(stream);
                    textures[i].unused1 = DataParser.ReadInt(stream);
                    textures[i].materialP = DataParser.ReadInt(stream);
                    textures[i].clientMaterialP = DataParser.ReadInt(stream);

                    textures[i].unused = new int[10];
                    for (int j = 0; j < textures[i].unused.Length; j++)
                    {
                        textures[i].unused[j] = DataParser.ReadInt(stream);
                    }

                    nextTexturePosition = stream.Position;

                    if (textures[i].nameOffset != 0)
                    {
                        stream.Position = texturePosition + textures[i].nameOffset;
                        textures[i].name = DataParser.ReadNullTerminatedString(stream);
                    }
                    else textures[i].name = "";
                }
            }

            return textures;
        }
        private static string[] ParseTexturePaths(Stream stream, long fileBeginOffset, studiohdr_t header1)
        {
            string[] texturePaths = null;
            if (header1.texturedir_count >= 0)
            {
                long nextTextureDirPosition = fileBeginOffset + header1.texturedir_offset;

                texturePaths = new string[header1.texturedir_count];
                for (int i = 0; i < texturePaths.Length; i++)
                {
                    stream.Position = nextTextureDirPosition;
                    int texturePathPosition = DataParser.ReadInt(stream);

                    nextTextureDirPosition = stream.Position;

                    if (texturePathPosition != 0)
                    {
                        stream.Position = fileBeginOffset + texturePathPosition;
                        texturePaths[i] = DataParser.ReadNullTerminatedString(stream);
                    }
                    else texturePaths[i] = "";
                }
            }

            return texturePaths;
        }
    }
}