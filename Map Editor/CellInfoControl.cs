using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Map_Editor
{
    public partial class CellInfoControl : UserControl
    {
        public CellInfoControl()
        {
            InitializeComponent();
        }

        public void SetText(int x, int y, int backImageIndex, int middleImageIndex, int frontImageIndex,int bLibIndex,int mLibIndex,int fLibIndex,string bLibName,string mLibName,string fLibName,int backLimit,int frontLimit,
            byte fFrame, byte ftick, bool fblend, byte mFrame, byte mTick, bool mBlend,byte doorOffSet,byte doorIndex,bool entityDoor,byte light,bool fishing)
        {
            LabX.Text = x.ToString();
            labY.Text = y.ToString();
            labBackImageIndex.Text = backImageIndex.ToString();
            labMiddleImageIndex.Text = middleImageIndex.ToString();
            labFrontImageIndex.Text = frontImageIndex.ToString();
            if ((bLibIndex >= 0) && (bLibIndex <= 99))
            {
                labBVersion.Text = "WemadeMir2";
            }
            else if (bLibIndex >= 100 && (bLibIndex <= 199))
            {
                labBVersion.Text = "ShandaMir2";
            }
            else if (bLibIndex >= 200 && (bLibIndex <= 299))
            {
                labBVersion.Text = "WemadeMir3";
            }
            else if (bLibIndex >= 300 && (bLibIndex <= 399))
            {
                labBVersion.Text = "ShandaMir3";
            }
            else
            {
                labBVersion.Text = "";
            }

            if ((mLibIndex >= 0) && (mLibIndex <= 99))
            {
                labMVersion.Text = "WemadeMir2";
            }
            else if (mLibIndex >= 100 && (mLibIndex <= 199))
            {
                labMVersion.Text = "ShandaMir2";
            }
            else if (mLibIndex >= 200 && (mLibIndex <= 299))
            {
                labMVersion.Text = "WemadeMir3";
            }
            else if (mLibIndex >= 300 && (mLibIndex <= 399))
            {
                labMVersion.Text = "ShandaMir3";
            }
            else
            {
                labMVersion.Text = "";
            }

            if ((fLibIndex >= 0) && (fLibIndex <= 99))
            {
                labFVersion.Text = "WemadeMir2";
            }
            else if (fLibIndex >= 100 && (fLibIndex <= 199))
            {
                labFVersion.Text = "ShandaMir2";
            }
            else if (fLibIndex >= 200 && (fLibIndex <= 299))
            {
                labFVersion.Text = "WemadeMir3";
            }
            else if (fLibIndex >= 300 && (fLibIndex <= 399))
            {
                labFVersion.Text = "ShandaMir3";
            }
            else
            {
                labFVersion.Text = "";
            }

            labBLibIndex.Text = bLibIndex.ToString();
            labMLibIndex.Text = mLibIndex.ToString();
            labFLibIndex.Text = fLibIndex.ToString();

            labBLibName.Text = bLibName;
            labMLibName.Text = mLibName;
            labFLibName.Text = fLibName;

            if (backLimit!=0)
            {
                LabBackLimit.Text = "True";
            }
            else
            {
                LabBackLimit.Text = "False";
            }
            if (frontLimit != 0)
            {
                labFrontLimit.Text = "True";
            }
            else
            {
                labFrontLimit.Text = "False";
            }


            if (fFrame>0)
            {
                labFFrame.Text = fFrame.ToString();
                labFTick.Text = ftick.ToString();
                labFBlend.Text = fblend.ToString();
            }
            else
            {
                labFFrame.Text = String.Empty;
                labFTick.Text = String.Empty;
                labFBlend.Text = String.Empty;
            }
            if ((mFrame>0)&&(mFrame<255))
            {
                labMFrame.Text = (mFrame&0x0F).ToString();
                labMTick.Text = mTick.ToString();
                labMBlend.Text = Convert.ToBoolean(mFrame & 0x0F).ToString();
            }
            else
            {
                labMFrame.Text = String.Empty;
                labMTick.Text = String.Empty;
                labMBlend.Text = String.Empty;
            }

            labDoorOffSet.Text = doorOffSet.ToString();
            labDoorIndex.Text = doorIndex.ToString();
            labEntityDoor.Text = entityDoor.ToString();

            labLight.Text = light.ToString();
            labfishing.Text = fishing.ToString();
        }
    }
}
