﻿namespace KangaModeling.Compiler.SequenceDiagrams
{
    internal class EnsureParticipantStatement : SimpleParticipantStatement
    {
        public EnsureParticipantStatement(Token name) 
            : base(name, name)
        {
        }

        public override void Build(ModelBuilder builder)
        {
            if (!builder.HasParticipant(Id.Value))
            {
                base.Build(builder);
            }
        }
    }
}