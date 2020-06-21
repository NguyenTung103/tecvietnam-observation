// Decompiled with JetBrains decompiler
// Type: bts.udpgateway.integration.DataMongo
// Assembly: bts.udpgateway.integration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0888BFEA-67EB-4BDD-A416-F9FFB08D390D
// Assembly location: E:\TEC\bts.udpgateway.integration.dll

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace bts.udpgateway.integration
{
  public class DataMongo
  {
    [BsonId]
    public ObjectId _id { get; set; }

    public DateTime DateCreate { get; set; }

    public bool IsSEQ { get; set; }

    public int Device_Id { get; set; }

    public double BTI { get; set; }

    public double BT1 { get; set; }

    public double BHU { get; set; }

    public double BTO { get; set; }

    public double BDR { get; set; }

    public double BFL { get; set; }

    public double BFR { get; set; }

    public double BPS { get; set; }

    public double BAV { get; set; }

    public double BAP { get; set; }

    public double BAC { get; set; }

    public double BAF { get; set; }

    public double BV1 { get; set; }

    public double BC1 { get; set; }

    public double BV2 { get; set; }

    public double BC2 { get; set; }

    public double BT2 { get; set; }

    public double BSE { get; set; }

    public double BA1 { get; set; }

    public double BB1 { get; set; }

    public double BA2 { get; set; }

    public double BB2 { get; set; }

    public double BA3 { get; set; }

    public double BB3 { get; set; }

    public double BA4 { get; set; }

    public double BB4 { get; set; }

    public double BFA { get; set; }

    public double BFD { get; set; }

    public double BPW { get; set; }

    public double BWS { get; set; }
  }
}
