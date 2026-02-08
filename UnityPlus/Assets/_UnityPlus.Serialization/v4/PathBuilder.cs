
using System.Text;

namespace UnityPlus.Serialization
{
    public static class PathBuilder
    {
        public static string BuildPath( ExecutionStack stack )
        {
            if( stack == null || stack.Count == 0 ) return "Root";

            var sb = new StringBuilder();
            var frames = stack.Frames;

            for( int i = 0; i < frames.Count; i++ )
            {
                var cursor = frames[i];

                if( i == 0 )
                {
                    sb.Append( "Root" );
                    continue;
                }

                IMemberInfo member = cursor.Tracker.Member;
                if( member != null )
                {
                    if( member.Name != null )
                    {
                        sb.Append( '.' );
                        sb.Append( member.Name );
                    }
                    else if( member.Index != -1 )
                    {
                        sb.Append( '[' );
                        sb.Append( member.Index );
                        sb.Append( ']' );
                    }
                    else
                    {
                        sb.Append( ".?" );
                    }
                }
            }

            return sb.ToString();
        }
    }
}
