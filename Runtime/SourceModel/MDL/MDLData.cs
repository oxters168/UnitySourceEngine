namespace UnitySourceEngine
{
    public struct MDLData
    {
        public studiohdr_t header1; //354+(2*n.length) bytes
        public studiohdr2_t header2; //32+(4*r.length) bytes
        public mstudiobone_t[] bones; //(160+(2*n.length)+(2*tspn.length)+(4*bci.length))*b.length bytes
        public mstudiobodyparts_t[] bodyParts;
        public mstudioattachment_t[] attachments;
        public mstudioanimdesc_t[] animDescs;
        public mstudiotexture_t[] textures;
        public string[] texturePaths;

        public ulong CountBytes()
        {
            ulong totalBytes = header1.CountBytes() + header2.CountBytes();
            if (bones != null)
                foreach(var bone in bones)
                    totalBytes += bone.CountBytes();
            if (bodyParts != null)
                foreach(var bodyPart in bodyParts)
                    totalBytes += bodyPart.CountBytes();
            if (attachments != null)
                foreach(var attachment in attachments)
                    totalBytes += attachment.CountBytes();
            if (animDescs != null)
                foreach(var animDesc in animDescs)
                    totalBytes += animDesc.CountBytes();
            if (textures != null)
                foreach(var texture in textures)
                    totalBytes += texture.CountBytes();
            if (texturePaths != null)
                foreach(var texturePath in texturePaths)
                    totalBytes += (ulong)(2*texturePath.Length);
            return totalBytes;
        }
    }
}