/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 0.0.1
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */

namespace CppApi {

using System;
using System.Runtime.InteropServices;

public class VectorConsumer : IDisposable {
  private HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal VectorConsumer(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(VectorConsumer obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~VectorConsumer() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          CppApiPINVOKE.delete_VectorConsumer(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
    }
  }

  public VectorConsumer() : this(CppApiPINVOKE.new_VectorConsumer(), true) {
    SwigDirectorConnect();
  }

  public virtual int VectorTakerPtr3(ref Fusee.Math.Core.Vector3D /* cstype */ pVec) {
    int ret = (SwigDerivedClassHasMethod("VectorTakerPtr3", swigMethodTypes0) ? CppApiPINVOKE.VectorConsumer_VectorTakerPtr3SwigExplicitVectorConsumer(swigCPtr, ref pVec /* csin */) : CppApiPINVOKE.VectorConsumer_VectorTakerPtr3(swigCPtr, ref pVec /* csin */));
    return ret;
  }

  public virtual int VectorTakerRef3(ref Fusee.Math.Core.Vector3D /* cstype */ rVec) {
    int ret = (SwigDerivedClassHasMethod("VectorTakerRef3", swigMethodTypes1) ? CppApiPINVOKE.VectorConsumer_VectorTakerRef3SwigExplicitVectorConsumer(swigCPtr, ref rVec /* csin */) : CppApiPINVOKE.VectorConsumer_VectorTakerRef3(swigCPtr, ref rVec /* csin */));
    return ret;
  }

  public virtual int VectorTakerVal3(Fusee.Math.Core.Vector3D /* CVector3cstype */ vec) {
    int ret = (SwigDerivedClassHasMethod("VectorTakerVal3", swigMethodTypes2) ? CppApiPINVOKE.VectorConsumer_VectorTakerVal3SwigExplicitVectorConsumer(swigCPtr, vec /* CVector3_csin */) : CppApiPINVOKE.VectorConsumer_VectorTakerVal3(swigCPtr, vec /* CVector3_csin */));
    return ret;
  }

  public virtual Fusee.Math.Core.Vector3D /* CVector3_cstype_out */ GimmeSomeVector()  {  /* <CVector3_csout> */
      Fusee.Math.Core.Vector3D ret = (SwigDerivedClassHasMethod("GimmeSomeVector", swigMethodTypes3) ? CppApiPINVOKE.VectorConsumer_GimmeSomeVectorSwigExplicitVectorConsumer(swigCPtr) : CppApiPINVOKE.VectorConsumer_GimmeSomeVector(swigCPtr));
      return ret;
   } /* <CVector3_csout> */ 

  public Fusee.Math.Core.Vector3D /* CVector3_cstype_out */ VV {
    /* <CVector3_csvarin> */
    set 
	{
      CppApiPINVOKE.VectorConsumer_VV_set(swigCPtr, value /* CVector3_csin */);
    }  /* </CVector3_csvarin> */   
   /* <CVector3_csvarout> */
   get
   {  
      Fusee.Math.Core.Vector3D ret = CppApiPINVOKE.VectorConsumer_VV_get(swigCPtr);
      return ret;
   } /* <CVector3_csvarout> */ 
  }

  public virtual int VectorTakerPtr4(ref Fusee.Math.Core.Vector4D /* cstype */ pVec) {
    int ret = (SwigDerivedClassHasMethod("VectorTakerPtr4", swigMethodTypes4) ? CppApiPINVOKE.VectorConsumer_VectorTakerPtr4SwigExplicitVectorConsumer(swigCPtr, ref pVec /* csin */) : CppApiPINVOKE.VectorConsumer_VectorTakerPtr4(swigCPtr, ref pVec /* csin */));
    return ret;
  }

  public virtual int VectorTakerRef4(ref Fusee.Math.Core.Vector4D /* cstype */ rVec) {
    int ret = (SwigDerivedClassHasMethod("VectorTakerRef4", swigMethodTypes5) ? CppApiPINVOKE.VectorConsumer_VectorTakerRef4SwigExplicitVectorConsumer(swigCPtr, ref rVec /* csin */) : CppApiPINVOKE.VectorConsumer_VectorTakerRef4(swigCPtr, ref rVec /* csin */));
    return ret;
  }

  public virtual int VectorTakerVal4(Fusee.Math.Core.Vector4D /* cstype */ vec) {
    int ret = (SwigDerivedClassHasMethod("VectorTakerVal4", swigMethodTypes6) ? CppApiPINVOKE.VectorConsumer_VectorTakerVal4SwigExplicitVectorConsumer(swigCPtr, ref vec /* csin */) : CppApiPINVOKE.VectorConsumer_VectorTakerVal4(swigCPtr, ref vec /* csin */));
    if (CppApiPINVOKE.SWIGPendingException.Pending) throw CppApiPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private void SwigDirectorConnect() {
    if (SwigDerivedClassHasMethod("VectorTakerPtr3", swigMethodTypes0))
      swigDelegate0 = new SwigDelegateVectorConsumer_0(SwigDirectorVectorTakerPtr3);
    if (SwigDerivedClassHasMethod("VectorTakerRef3", swigMethodTypes1))
      swigDelegate1 = new SwigDelegateVectorConsumer_1(SwigDirectorVectorTakerRef3);
    if (SwigDerivedClassHasMethod("VectorTakerVal3", swigMethodTypes2))
      swigDelegate2 = new SwigDelegateVectorConsumer_2(SwigDirectorVectorTakerVal3);
    if (SwigDerivedClassHasMethod("GimmeSomeVector", swigMethodTypes3))
      swigDelegate3 = new SwigDelegateVectorConsumer_3(SwigDirectorGimmeSomeVector);
    if (SwigDerivedClassHasMethod("VectorTakerPtr4", swigMethodTypes4))
      swigDelegate4 = new SwigDelegateVectorConsumer_4(SwigDirectorVectorTakerPtr4);
    if (SwigDerivedClassHasMethod("VectorTakerRef4", swigMethodTypes5))
      swigDelegate5 = new SwigDelegateVectorConsumer_5(SwigDirectorVectorTakerRef4);
    if (SwigDerivedClassHasMethod("VectorTakerVal4", swigMethodTypes6))
      swigDelegate6 = new SwigDelegateVectorConsumer_6(SwigDirectorVectorTakerVal4);
    CppApiPINVOKE.VectorConsumer_director_connect(swigCPtr, swigDelegate0, swigDelegate1, swigDelegate2, swigDelegate3, swigDelegate4, swigDelegate5, swigDelegate6);
  }

  private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes) {
    System.Reflection.MethodInfo methodInfo = this.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, methodTypes, null);
    bool hasDerivedMethod = methodInfo.DeclaringType.IsSubclassOf(typeof(VectorConsumer));
    return hasDerivedMethod;
  }

  private int SwigDirectorVectorTakerPtr3(IntPtr pVec) {
       
    Fusee.Math.Core.Vector3D vec_pVec;
    unsafe {vec_pVec = Fusee.Math.ArrayConversion.Convert.ArrayDoubleToVector3D((double *)pVec);}
    /* csdirectorin_pre */
    try {
       return VectorTakerPtr3(ref vec_pVec /* csdirectorin */);
    }
    finally {
        unsafe {Fusee.Math.ArrayConversion.Convert.Vector3DToArrayDouble(vec_pVec, (double *)pVec);}
        /* csdirectorin_post */
    }
  }

  private int SwigDirectorVectorTakerRef3(IntPtr rVec) {
       
    Fusee.Math.Core.Vector3D vec_rVec;
    unsafe {vec_rVec = Fusee.Math.ArrayConversion.Convert.ArrayDoubleToVector3D((double *)rVec);}
    /* csdirectorin_pre */
    try {
       return VectorTakerRef3(ref vec_rVec /* csdirectorin */);
    }
    finally {
        unsafe {Fusee.Math.ArrayConversion.Convert.Vector3DToArrayDouble(vec_rVec, (double *)rVec);}
        /* csdirectorin_post */
    }
  }

  private int SwigDirectorVectorTakerVal3(Fusee.Math.Core.Vector3D /* CVector3_imtype_out */ vec) {
       
/* NOP CVector3_csdirectorin_pre */
          return VectorTakerVal3(vec /* CVector3_csdirectorin */);
  }

  private Fusee.Math.Core.Vector3D /* CVector3_imtype_out */ SwigDirectorGimmeSomeVector() {
    return GimmeSomeVector() /* CVector3_csdirectorout */;
  }

  private int SwigDirectorVectorTakerPtr4(IntPtr pVec) {
       
    Fusee.Math.Core.Vector4D vec_pVec;
    unsafe {vec_pVec = Fusee.Math.ArrayConversion.Convert.ArrayDoubleToVector4D((double *)pVec);}
    /* csdirectorin_pre */
    try {
       return VectorTakerPtr4(ref vec_pVec /* csdirectorin */);
    }
    finally {
        unsafe {Fusee.Math.ArrayConversion.Convert.Vector4DToArrayDouble(vec_pVec, (double *)pVec);}
        /* csdirectorin_post */
    }
  }

  private int SwigDirectorVectorTakerRef4(IntPtr rVec) {
       
    Fusee.Math.Core.Vector4D vec_rVec;
    unsafe {vec_rVec = Fusee.Math.ArrayConversion.Convert.ArrayDoubleToVector4D((double *)rVec);}
    /* csdirectorin_pre */
    try {
       return VectorTakerRef4(ref vec_rVec /* csdirectorin */);
    }
    finally {
        unsafe {Fusee.Math.ArrayConversion.Convert.Vector4DToArrayDouble(vec_rVec, (double *)rVec);}
        /* csdirectorin_post */
    }
  }

  private int SwigDirectorVectorTakerVal4(IntPtr vec) {
       
    Fusee.Math.Core.Vector4D vec_vec;
    unsafe {vec_vec = Fusee.Math.ArrayConversion.Convert.ArrayDoubleToVector4D((double *)vec);}
    /* csdirectorin_pre */
          return VectorTakerVal4(vec_vec /* csdirectorin */);
  }

  public delegate int SwigDelegateVectorConsumer_0(IntPtr pVec);
  public delegate int SwigDelegateVectorConsumer_1(IntPtr rVec);
  public delegate int SwigDelegateVectorConsumer_2(Fusee.Math.Core.Vector3D /* CVector3_imtype_out */ vec);
  public delegate Fusee.Math.Core.Vector3D /* CVector3_imtype_out */ SwigDelegateVectorConsumer_3();
  public delegate int SwigDelegateVectorConsumer_4(IntPtr pVec);
  public delegate int SwigDelegateVectorConsumer_5(IntPtr rVec);
  public delegate int SwigDelegateVectorConsumer_6(IntPtr vec);

  private SwigDelegateVectorConsumer_0 swigDelegate0;
  private SwigDelegateVectorConsumer_1 swigDelegate1;
  private SwigDelegateVectorConsumer_2 swigDelegate2;
  private SwigDelegateVectorConsumer_3 swigDelegate3;
  private SwigDelegateVectorConsumer_4 swigDelegate4;
  private SwigDelegateVectorConsumer_5 swigDelegate5;
  private SwigDelegateVectorConsumer_6 swigDelegate6;

  private static Type[] swigMethodTypes0 = new Type[] { typeof(Fusee.Math.Core.Vector3D /* cstype */).MakeByRefType() };
  private static Type[] swigMethodTypes1 = new Type[] { typeof(Fusee.Math.Core.Vector3D /* cstype */).MakeByRefType() };
  private static Type[] swigMethodTypes2 = new Type[] { typeof(Fusee.Math.Core.Vector3D /* CVector3cstype */) };
  private static Type[] swigMethodTypes3 = new Type[] {  };
  private static Type[] swigMethodTypes4 = new Type[] { typeof(Fusee.Math.Core.Vector4D /* cstype */).MakeByRefType() };
  private static Type[] swigMethodTypes5 = new Type[] { typeof(Fusee.Math.Core.Vector4D /* cstype */).MakeByRefType() };
  private static Type[] swigMethodTypes6 = new Type[] { typeof(Fusee.Math.Core.Vector4D /* cstype */) };
}

}
