using GoogleARCore;
using GoogleARCoreInternal;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CameraMetadata : MonoBehaviour {

    public static void GetCameraMetadata ()
    {
        StringBuilder stringBuiler = new StringBuilder();
        List<CameraMetadataTag> tags = new List<CameraMetadataTag>();
        List<CameraMetadataValue> values = new List<CameraMetadataValue>();
        if (Frame.CameraMetadata.GetAllCameraMetadataTags(tags))
        {
            foreach (CameraMetadataTag tag in tags)
            {
                stringBuiler.AppendFormat("tag: {0}\n", tag.ToString());
                if (Frame.CameraMetadata.TryGetValues(tag, values))
                {
                    foreach (CameraMetadataValue val in values)
                    {
                        System.Type type = val.ValueType;
                        stringBuiler.AppendFormat("\tvalue: {0} ({1})\n", GetCameraMetadataValue(val), val.m_Type.ToString());
                    }

                }
                Debug.Log(stringBuiler.ToString());
                stringBuiler = new StringBuilder();
            }
        }
    }

    private static object GetCameraMetadataValue(CameraMetadataValue value)
    {
        switch (value.m_Type)
        {
            case NdkCameraMetadataType.Byte:
                return value.AsByte();
            case NdkCameraMetadataType.Int32:
                return value.AsInt();
            case NdkCameraMetadataType.Float:
                return value.AsFloat();
            case NdkCameraMetadataType.Int64:
                return value.AsLong();
            case NdkCameraMetadataType.Double:
                return value.AsDouble();
            case NdkCameraMetadataType.Rational:
                var r = value.AsRational();
                return r.Numerator.ToString() + "/" + r.Denominator.ToString();
            default:
                return null;
        }
    }
}
