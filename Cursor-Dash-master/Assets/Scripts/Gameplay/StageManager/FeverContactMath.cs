using System;

/// <summary>Fever-only continuous box/circle contact. Point segment is expressed in the box's local axes.</summary>
public static class FeverContactMath
{
    public static bool Sweep(double x0,double y0,double x1,double y1,double halfX,double halfY,double radius,out double hit)
    {
        hit=1;
        if(double.IsNaN(x0+x1+y0+y1+halfX+halfY+radius)||double.IsInfinity(x0+x1+y0+y1+halfX+halfY+radius))return false;
        halfX=Math.Max(0,halfX);halfY=Math.Max(0,halfY);radius=Math.Max(0,radius);
        double dx=x1-x0,dy=y1-y0,best=double.PositiveInfinity;
        double qx=Math.Max(0,Math.Abs(x0)-halfX),qy=Math.Max(0,Math.Abs(y0)-halfY);
        if(qx*qx+qy*qy<=radius*radius+1e-12){hit=0;return true;}
        void Rect(double hx,double hy){
            double lo=0,hi=1;
            bool Axis(double p,double v,double h){if(Math.Abs(v)<1e-12)return Math.Abs(p)<=h;double a=(-h-p)/v,b=(h-p)/v;if(a>b){double swap=a;a=b;b=swap;}lo=Math.Max(lo,a);hi=Math.Min(hi,b);return lo<=hi+1e-12;}
            if(Axis(x0,dx,hx)&&Axis(y0,dy,hy)&&lo<=1&&hi>=0)best=Math.Min(best,Math.Max(0,lo));
        }
        // The Minkowski sum is two strips and four round corners, not an oversized square.
        Rect(halfX+radius,halfY);Rect(halfX,halfY+radius);
        double a2=dx*dx+dy*dy;
        if(a2>1e-20)for(int sx=-1;sx<=1;sx+=2)for(int sy=-1;sy<=1;sy+=2){
            double x=x0-sx*halfX,y=y0-sy*halfY,b=x*dx+y*dy,c=x*x+y*y-radius*radius,disc=b*b-a2*c;
            if(disc< -1e-12)continue;double t=(-b-Math.Sqrt(Math.Max(0,disc)))/a2;if(t>=-1e-12&&t<=1)best=Math.Min(best,Math.Max(0,t));
        }
        if(double.IsPositiveInfinity(best))return false;hit=Math.Min(1,best);return true;
    }
}
