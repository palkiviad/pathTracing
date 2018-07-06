using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PathFinder {
    
    public static class Glu
    {
        // Из сцены в экран
        public static bool GluProject(Vector3 objPos, out Vector3 screenPos)
        {
            Vector4 _in;

            _in.X = objPos.X;
            _in.Y = objPos.Y;
            _in.Z = objPos.Z;
            _in.W = 1f;
            
            var viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            
            Matrix4 modelMatrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out modelMatrix);

            Matrix4 projMatrix;
            GL.GetFloat(GetPName.ProjectionMatrix, out projMatrix);
            Matrix4 matWorldViewProjection = Matrix4.Mult(modelMatrix, projMatrix);
            Vector4 _out = Vector4.Transform(_in, matWorldViewProjection);

            if (_out.W <= 0.0)
            {
                screenPos = Vector3.Zero;
                return false;
            }

            _out.X /= _out.W;
            _out.Y /= _out.W;
            _out.Z /= _out.W;
            /* Map x, y and z to range 0-1 */
            _out.X = _out.X * 0.5f + 0.5f;
            _out.Y = -_out.Y * 0.5f + 0.5f;
            _out.Z = _out.Z * 0.5f + 0.5f;

            /* Map x,y to viewport */
            _out.X = _out.X * viewport[2] + viewport[0];
            _out.Y = _out.Y * viewport[3] + viewport[1];

            screenPos.X = _out.X;
            screenPos.Y = _out.Y;
            screenPos.Z = _out.Z;

            return true;
        }
        
        // из экрана в сцену
        public static bool UnProject(Vector3 win, ref Vector3 obj)
        {
            Matrix4 modelMatrix;
            GL.GetFloat(GetPName.ModelviewMatrix, out modelMatrix);

            Matrix4 projMatrix;
            GL.GetFloat(GetPName.ProjectionMatrix, out projMatrix);

            var viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);

            return UnProject(win, modelMatrix, projMatrix, viewport, ref obj);
        }

        private static bool UnProject(Vector3 win, Matrix4 modelMatrix, Matrix4 projMatrix, int[] viewport, ref Vector3 obj)
        {
            return GluUnProject(win.X, win.Y, win.Z, modelMatrix, projMatrix, viewport, ref obj.X, ref obj.Y, ref obj.Z);
        }

        private static bool GluUnProject(float winx, float winy, float winz, Matrix4 modelMatrix, Matrix4 projMatrix, int[] viewport, ref float objx, ref float objy, ref float objz)
        {
            Vector4 _in;

            Matrix4 finalMatrix = Matrix4.Mult(modelMatrix, projMatrix);

            finalMatrix.Invert();

            _in.X = winx;
            _in.Y = viewport[3] - winy;
            _in.Z = winz;
            _in.W = 1.0f;

            _in.X = (_in.X - viewport[0]) / viewport[2];
            _in.Y = (_in.Y - viewport[1]) / viewport[3];

            _in.X = _in.X * 2 - 1;
            _in.Y = _in.Y * 2 - 1;
            _in.Z = _in.Z * 2 - 1;

            Vector4 _out = Vector4.Transform(_in, finalMatrix);

            if (_out.W == 0.0)
                return false;
            
            _out.X /= _out.W;
            _out.Y /= _out.W;
            _out.Z /= _out.W;
            
            objx = _out.X;
            objy = _out.Y;
            objz = _out.Z;
            return true;
        }
    }
}