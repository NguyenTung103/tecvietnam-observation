// Decompiled with JetBrains decompiler
// Type: bts.udpgateway.integration.DataAlarmMongo
// Assembly: bts.udpgateway.integration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0888BFEA-67EB-4BDD-A416-F9FFB08D390D
// Assembly location: E:\TEC\bts.udpgateway.integration.dll

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace bts.udpgateway.integration
{
  public class DataAlarmMongo
  {
    [BsonId]
    public ObjectId _id { get; set; }

    public DateTime DateCreate { get; set; }

    public bool IsSEQ { get; set; }

    public int Device_Id { get; set; }

    public string AMATI { get; set; }

    public string AMIHU { get; set; }

    public string AMADR { get; set; }

    public string AMAFL { get; set; }

    public string AMAFR { get; set; }

    public string AMIPS { get; set; }

    public string AMIAL { get; set; }

    public string AMIAH { get; set; }

    public string AMIAP { get; set; }

    public string AMIAC { get; set; }

    public string AMIGN { get; set; }

    public string AMIAR { get; set; }

    public string AMIL1 { get; set; }

    public string AMIH1 { get; set; }

    public string AMIT1 { get; set; }

    public string AMIL2 { get; set; }

    public string AMIH2 { get; set; }

    public string AMIT2 { get; set; }
  }
}
