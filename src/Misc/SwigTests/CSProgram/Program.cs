﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CppApi;
using Fusee.Math.Core;

namespace CSProgram
{
    class MyVectorConsumer : VectorConsumer
    {
        public override int VectorTakerPtr3(ref Vector3D pVec)
        {
            return base.VectorTakerPtr3(ref pVec);
        }

        public override int VectorTakerRef3(ref Vector3D rVec)
        {
            rVec.Y = 666;
            return 333;
        }

        public override int VectorTakerVal3(Vector3D vec)
        {
            return base.VectorTakerVal3(vec);
        }

        
        public override int VectorTakerPtr4(ref Vector4D pVec)
        {
            return base.VectorTakerPtr4(ref pVec);
        }

        public override int VectorTakerRef4(ref Vector4D rVec)
        {
            rVec.W = 666;
            return 333;
        }

        public override int VectorTakerVal4(Vector4D vec)
        {
            return base.VectorTakerVal4(vec);
        }

        public override Vector3D GimmeSomeVector()
        {
            return new Vector3D(333, 666, 999);
        }

        //public override int MatrixTakerPtr(ref Matrix4D pVec)
        //{
        //    return base.MatrixTakerPtr(ref pVec);
        //}

        //public override int MatrixTakerRef(ref Matrix4D rVec)
        //{
        //    rVec.M33 = 333;
        //    return 333;
        //}
        /*
        public override int MatrixTakerVal(Matrix4D vec)
        {
            return base.MatrixTakerVal(vec);
        }
        */
    }

    class Program
    {
        static void Main(string[] args)
        {
            Child child = Factory.GimmeAChild();
            Parent parent = Factory.GimmeAParent();
            Parent parent2 = Factory.GimmeAChildAsAParent();

            child.DoEvenMore();
            child.DoSomething(3, 4);
            parent.DoSomething(2, 3);
            parent2.DoSomething(1, 2);
            Child child2 = (Child)parent2;

            // SIZE howbig = SIZE.MEDIUM;
            // parent2.MethodWithRefSize(ref howbig);
            Vector3D v3 = new Vector3D(11, 22, 33);
            Vector4D v4 = new Vector4D(1, 2, 3, 4);
            Matrix4D mtx = new Matrix4D(11, 12, 13, 14, 21, 22, 23, 24, 31, 32, 33, 34, 41, 42, 43, 44);
            int res;

            // Handmade Matrix and Vector wrapping test
            // Note: This tests the mapping of C++ Matrix and Vector types
            // to exsiting C# Matrix and Vector types (and not wrapper classes)
            res = HandmadeWrappers.HandMadeVectorTaker(ref v4);
            //res = HandmadeWrappers.HandMadeMatrixTakerWrapper(ref mtx);
            Vector4D vRes = HandmadeWrappers.HandMadeVectorReturner();
            //Matrix4D mRes = HandmadeWrappers.HandMadeMatrixReturnerWrapper();
            Vector4D vRes2 = HandmadeWrappers.HandMadeVectorPtrReturnerWrapper();


            
            // Swig generated Matrix and Vector wrapping test
            // Note: This tests the mapping of C++ Matrix and Vector types
            // to exsiting C# Matrix and Vector types (and not wrapper classes generated by Swig).
            // The mapping of the types is done by Swig, though
            VectorConsumer vc = new VectorConsumer();
            //Matrix4D mRes2 = vc.GimmeSomeMatrix();

            res = vc.VectorTakerPtr3(ref v3);
            res = vc.VectorTakerRef3(ref v3);
            res = vc.VectorTakerVal3(v3);
            Vector3D vRet = vc.GimmeSomeVector();
            Vector3D vOld = vc.VV;
            vc.VV = vRet;
            vOld = vc.VV;


            res = vc.VectorTakerPtr4(ref v4);
            res = vc.VectorTakerRef4(ref v4);
            res = vc.VectorTakerVal4(v4);
            //res = vc.MatrixTakerPtr(ref mtx);
            //res = vc.MatrixTakerRef(ref mtx);
            //res = vc.MatrixTakerVal(mtx);


            MyVectorConsumer myVc = new MyVectorConsumer();
            VectorConsumerCaller.CallVectorConsumer(myVc);


            AParamType param = null;
            RefRefTest.ParameterTaker(ref param);
            int i = 8;
        }
    }
}