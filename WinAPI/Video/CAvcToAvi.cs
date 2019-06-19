using System;
using System.IO;
using System.Runtime.InteropServices;
using static CommonLib.WinAPI.Video.bs;

namespace CommonLib.WinAPI.Video
{

    public unsafe class CAvcToAvi : IDisposable
    {
        const int DATA_MAX = 3000000;
        private const uint AVIF_HASINDEX = 0x00000010; // Index at end of file?
        private const uint AVIF_ISINTERLEAVED = 0x00000100;
        private const uint AVIF_TRUSTCKTYPE = 0x00000800; // Use CKType to find key frames?
        private const long AVIIF_KEYFRAME = 0x00000010L; /* this frame is a key frame.*/
        private cfg_t cfg;
        private avi_t avi;
        private h264_t h264;
        private nal_t nal;
        private vbuf_t vb;
        private FileStream fin;
        private FileStream fout;
        private int b_eof;
        private int b_key;
        private int b_slice;
        private int i_frame;
        private int i_data;
        private byte* data;
        private int type;
        private int i_data2;
        private readonly string _fileName;
        public CAvcToAvi()
        {
            cfg = new cfg_t();
            cfg.f_fps = 24;
            cfg.fcc = "h264";

            /* Init data */
            b_eof = 0;
            b_key = 0;
            b_slice = 0;
            i_frame = 0;
            i_data = 0;

            avi = new avi_t();
            h264 = new h264_t();
            nal = new nal_t();
            nal.p_payload = (byte*)Marshal.AllocHGlobal(DATA_MAX).ToPointer();
            vb = new vbuf_t();

            data = (byte*)Marshal.AllocHGlobal(DATA_MAX).ToPointer();
        }
        /// <summary>
        /// 创建按帧存储的avi转换库
        /// </summary>
        /// <param name="saveFileName"></param>
        public CAvcToAvi(string saveFileName) : this()
        {
            _fileName = saveFileName;

            fout = File.OpenWrite(saveFileName + "_tmp");

            /* Init avi */
            avi_init(avi, fout, cfg.f_fps, cfg.fcc);

            /* Init parser */
            h264_parser_init(h264);

            vbuf_init(vb);
        }

        public void ToAvi_PumpVideoFrame(byte[] pFrame, int nLen)
        {
            if (pFrame == null || nLen < 3 || pFrame.Length < nLen) return;

            if (nLen > DATA_MAX)
                data = (byte*)Marshal.ReAllocHGlobal(new IntPtr(data), new IntPtr(nLen)).ToPointer();
            i_data = 0;
            Marshal.Copy(pFrame, 0, new IntPtr(&data[i_data]), nLen);
            i_data = nLen;
            /* split frame */
            while (true)
            {
                byte* p;
                byte* p_next;
                byte* end;
                int i_size;

                if (i_data < 3)
                    break;

                end = &data[i_data];
                /* Search begin of a NAL */
                p = &data[0];
                while (p < end - 3)
                {
                    if (p[0] == 0x00 && p[1] == 0x00 && p[2] == 0x01 /*&& p[3]==0x01*/)
                    {
                        break;
                    }
                    p++;
                }

                if (p >= end - 3)
                {
                    i_data = 0;
                    continue;
                }

                /* Search end of NAL */
                p_next = p + 3;
                while (p_next < end - 3)
                {
                    if (p_next[0] == 0x00 && p_next[1] == 0x00 && p_next[2] == 0x01 /*&& p[3]==0x01*/)
                    {
                        break;
                    }
                    p_next++;
                }

                if (p_next == end - 3 && i_data < DATA_MAX)
                    p_next = end;

                /* Compute NAL size */
                i_size = (int)(p_next - p - 3);
                if (i_size <= 0)
                {
                    if (b_eof != 0)
                        break;

                    i_data = 0;
                    continue;
                }

                /* Nal start at p+3 with i_size length */
                nal_decode(nal, p + 3, i_size < 2048 ? i_size : 2048);

                b_key = h264.b_key;

                type = nal.i_type; //tangxiaojun
                                   //if (1!=type) 
                                   //{
                                   //	int gfzfgd=8756;
                                   //}


                i_data2 = vb.i_data; //tangxiaojun

                if (b_slice != 0 && vb.i_data != 0 &&
                    (nal.i_type == (int)nal_unit_type_e.NAL_SPS || nal.i_type == (int)nal_unit_type_e.NAL_PPS))
                {
                    avi_write(avi, vb, b_key);
                    vbuf_reset(vb);
                    b_slice = 0;
                }

                fixed (int* _b_slice = &b_slice)
                {
                    /* Parse SPS/PPS/Slice */
                    if (ParseNAL(nal, avi, h264, _b_slice) != 0 && vb.i_data > 0)
                    {
                        avi_write(avi, vb, b_key);
                        vbuf_reset(vb);
                    }
                }

                /* fprintf( stderr, "nal:%d ref:%d\n", nal.i_type, nal.i_ref_idc ); */

                /* Append NAL to buffer */
                vbuf_add(vb, i_size + 3, p);

                /* Remove this nal */
                memmove(&data[0], p_next, (uint)(end - p_next));

                i_data -= (int)(p_next - &data[0]);
            }
        }

        public void ToAvi_EndFileWrite()
        {
            if (vb.i_data > 0)
            {
                avi_write(avi, vb, h264.b_key);
            }

            avi.i_width = h264.i_width;
            avi.i_height = h264.i_height;
            if (h264.vui_flag)
            {
                avi.f_fps = ((float)h264.i_time_scale / (h264.i_tick * 2));
            }

            avi_end(avi);
            fout?.Close();
            fout = null;
            File.Delete(_fileName);
            File.Move(_fileName + "_tmp", _fileName);
        }

        public bool H264ToAvi(string h264file, string avifile)
        {
            fin = File.OpenRead(h264file);
            fout = File.OpenWrite(avifile);

            /* Init avi */
            avi_init(avi, fout, cfg.f_fps, cfg.fcc);

            /* Init parser */
            h264_parser_init(h264);

            vbuf_init(vb);

            /* split frame */
            while (true)
            {
                byte* p;
                byte* p_next;
                byte* end;
                int i_size;

                /* fill buffer */
                if (i_data < DATA_MAX && b_eof == 0)
                {
                    byte[] buffer = new byte[1 * (DATA_MAX - i_data)];
                    int i_read = fin.Read(buffer, 0, buffer.Length);
                    Marshal.Copy(buffer, 0, new IntPtr(&data[i_data]), buffer.Length);
                    if (i_read <= 0)
                        b_eof = 1;
                    else
                        i_data += i_read;
                }
                if (i_data < 3)
                    break;

                end = &data[i_data];

                /* Search begin of a NAL */
                p = &data[0];
                while (p < end - 3)
                {
                    if (p[0] == 0x00 && p[1] == 0x00 && p[2] == 0x01 /*&& p[3]==0x01*/)
                    {
                        break;
                    }
                    p++;
                }

                if (p >= end - 3)
                {
                    i_data = 0;
                    continue;
                }

                /* Search end of NAL */
                p_next = p + 3;
                while (p_next < end - 3)
                {
                    if (p_next[0] == 0x00 && p_next[1] == 0x00 && p_next[2] == 0x01 /*&& p[3]==0x01*/)
                    {
                        break;
                    }
                    p_next++;
                }

                if (p_next == end - 3 && i_data < DATA_MAX)
                    p_next = end;

                /* Compute NAL size */
                i_size = (int)(p_next - p - 3);
                if (i_size <= 0)
                {
                    if (b_eof != 0)
                        break;

                    i_data = 0;
                    continue;
                }

                /* Nal start at p+3 with i_size length */
                nal_decode(nal, p + 3, i_size < 2048 ? i_size : 2048);

                b_key = h264.b_key;

                type = nal.i_type; //tangxiaojun
                                   //if (1!=type) 
                                   //{
                                   //	int gfzfgd=8756;
                                   //}


                i_data2 = vb.i_data; //tangxiaojun

                if (b_slice != 0 && vb.i_data != 0 &&
                    (nal.i_type == (int)nal_unit_type_e.NAL_SPS || nal.i_type == (int)nal_unit_type_e.NAL_PPS))
                {
                    avi_write(avi, vb, b_key);
                    vbuf_reset(vb);
                    b_slice = 0;
                }

                fixed (int* _b_slice = &b_slice)
                {
                    /* Parse SPS/PPS/Slice */
                    if (ParseNAL(nal, avi, h264, _b_slice) != 0 && vb.i_data > 0)
                    {
                        avi_write(avi, vb, b_key);
                        vbuf_reset(vb);
                    }
                }

                /* fprintf( stderr, "nal:%d ref:%d\n", nal.i_type, nal.i_ref_idc ); */

                /* Append NAL to buffer */
                vbuf_add(vb, i_size + 3, p);

                /* Remove this nal */
                memmove(&data[0], p_next, (uint)(end - p_next));

                i_data -= (int)(p_next - &data[0]);
            }

            if (vb.i_data > 0)
            {
                avi_write(avi, vb, h264.b_key);
            }

            avi.i_width = h264.i_width;
            avi.i_height = h264.i_height;
            if (h264.vui_flag)
            {
                avi.f_fps = ((float)h264.i_time_scale / (h264.i_tick * 2));
            }

            avi_end(avi);

            fin.Close();
            fout.Close();

            return true;
        }

        void avi_init(avi_t a, FileStream f, float f_fps, string fcc)
        {
            a.f = f;
            a.f_fps = f_fps;
            a.fcc = fcc;
            a.i_width = 0;
            a.i_height = 0;
            a.i_frame = 0;
            a.i_movi = 0;
            a.i_riff = 0;
            a.i_movi_end = 0;
            a.i_idx_max = 0;
            a.idx = null;

            avi_write_header(a);

            a.i_movi = a.f.Position;
        }

        void avi_write_header(avi_t a)
        {
            avi_write_fourcc(a, "RIFF");
            avi_write_uint32(a, a.i_riff > 0 ? (uint)a.i_riff - 8 : 0xFFFFFFFF);
            avi_write_fourcc(a, "AVI ");

            avi_write_fourcc(a, "LIST");
            avi_write_uint32(a, 4 + 4 * 16 + 12 + 4 * 16 + 4 * 12);
            avi_write_fourcc(a, "hdrl");

            avi_write_fourcc(a, "avih");
            avi_write_uint32(a, 4 * 16 - 8);
            avi_write_uint32(a, 1000000 / (uint)a.f_fps);
            avi_write_uint32(a, 0xffffffff);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, AVIF_HASINDEX | AVIF_ISINTERLEAVED | AVIF_TRUSTCKTYPE);
            avi_write_uint32(a, (uint)a.i_frame);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 1);
            avi_write_uint32(a, 1000000);
            avi_write_uint32(a, (uint)a.i_width);
            avi_write_uint32(a, (uint)a.i_height);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);

            avi_write_fourcc(a, "LIST");
            avi_write_uint32(a, 4 + 4 * 16 + 4 * 12);
            avi_write_fourcc(a, "strl");

            avi_write_fourcc(a, "strh");
            avi_write_uint32(a, 4 * 16 - 8);
            avi_write_fourcc(a, "vids");
            avi_write_fourcc(a, a.fcc);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 1000);
            avi_write_uint32(a, (uint)a.f_fps * 1000);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, (uint)a.i_frame);
            avi_write_uint32(a, 1024 * 1024);
            avi_write_int32(a, -1);
            avi_write_uint32(a, (uint)(a.i_width * a.i_height));
            avi_write_uint32(a, 0);
            avi_write_uint16(a, (short)a.i_width);
            avi_write_uint16(a, (short)a.i_height);

            avi_write_fourcc(a, "strf");
            avi_write_uint32(a, 4 * 12 - 8);
            avi_write_uint32(a, 4 * 12 - 8);
            avi_write_uint32(a, (uint)a.i_width);
            avi_write_uint32(a, (uint)a.i_height);
            avi_write_uint16(a, 1);
            avi_write_uint16(a, 24);
            avi_write_fourcc(a, a.fcc);
            avi_write_uint32(a, (uint)(a.i_width * a.i_height));
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);
            avi_write_uint32(a, 0);

            avi_write_fourcc(a, "LIST");
            avi_write_uint32(a, a.i_movi_end > 0 ? (uint)a.i_movi_end - (uint)a.i_movi + 4 : 0xFFFFFFFF);
            avi_write_fourcc(a, "movi");
        }

        void avi_write_uint16(avi_t a, short w)
        {
            //a.f.Write(BitConverter.GetBytes(w), 0, 2);
            a.f.WriteByte((byte)(w & 0xff));
            a.f.WriteByte((byte)((w >> 8) & 0xff));
        }

        void avi_write_uint32(avi_t a, uint dw)
        {
            //a.f.Write(BitConverter.GetBytes(dw), 0, 4);
            a.f.WriteByte((byte)(dw & 0xff));
            a.f.WriteByte((byte)((dw >> 8) & 0xff));
            a.f.WriteByte((byte)((dw >> 16) & 0xff));
            a.f.WriteByte((byte)((dw >> 24) & 0xff));
        }
        void avi_write_int32(avi_t a, int dw)
        {
            //a.f.Write(BitConverter.GetBytes(dw), 0, 4);
            a.f.WriteByte((byte)(dw & 0xff));
            a.f.WriteByte((byte)((dw >> 8) & 0xff));
            a.f.WriteByte((byte)((dw >> 16) & 0xff));
            a.f.WriteByte((byte)((dw >> 24) & 0xff));
        }
        void avi_write_fourcc(avi_t a, string fcc)
        {
            a.f.WriteByte((byte)fcc[0]);
            a.f.WriteByte((byte)fcc[1]);
            a.f.WriteByte((byte)fcc[2]);
            a.f.WriteByte((byte)fcc[3]);
        }

        void h264_parser_init(h264_t h)
        {
            h.i_width = 0;
            h.i_height = 0;
            h.b_key = 0;
            h.i_nal_type = -1;
            h.i_ref_idc = -1;
            h.i_idr_pic_id = -1;
            h.i_frame_num = -1;
            h.i_log2_max_frame_num = 0;
            h.i_poc = -1;
            h.i_poc_type = -1;
            h.vui_flag = false;
            h.i_tick = 0;
            h.i_time_scale = 0;
        }

        void vbuf_init(vbuf_t v)
        {
            v.i_data = 0;
            v.i_data_max = 10000;
            v.p_data = (byte*)Marshal.AllocHGlobal(v.i_data_max).ToPointer();
        }

        int nal_decode(nal_t nal, void* p_data, int i_data)
        {
            byte* src = (byte*)p_data;
            byte* end = &src[i_data];
            byte* dst = nal.p_payload;

            nal.i_type = src[0] & 0x1f;
            nal.i_ref_idc = (src[0] >> 5) & 0x03;

            src++;

            while (src < end)
            {
                if (src < end - 3 && src[0] == 0x00 && src[1] == 0x00 && src[2] == 0x03)
                {
                    *dst++ = 0x00;
                    *dst++ = 0x00;

                    src += 3;
                    continue;
                }
                *dst++ = *src++;
            }

            //nal.i_payload = dst - (uint8_t*)p_data;//tangxiaojun
            nal.i_payload = (int)(dst - nal.p_payload);//tangxiaojun
            return 0;
        }

        void avi_write(avi_t a, vbuf_t v, int b_key)
        {
            int i = 0;
            if(!a.f.CanWrite) return;
            long i_pos = a.f.Position;

            /* chunk header */
            avi_write_fourcc(a, "00dc");
            avi_write_uint32(a, (uint)v.i_data);

            byte[] buffer = new byte[v.i_data * 1];
            Marshal.Copy(new IntPtr(v.p_data), buffer, 0, buffer.Length);
            a.f.Write(buffer, 0, buffer.Length);

            if ((v.i_data & 0x01) != 0)
            {
                /* pad */
                a.f.WriteByte(0);
            }

            /* Append idx chunk */
            if (a.i_idx_max <= a.i_frame)
            {
                a.i_idx_max += 1000;
                if (a.idx == null)
                    a.idx = (uint*)Marshal.AllocHGlobal(a.i_idx_max * 16).ToPointer();
                else
                    a.idx = (uint*)Marshal.ReAllocHGlobal(new IntPtr(a.idx), new IntPtr(a.i_idx_max * 16)).ToPointer();
            }

            IntPtr tmpPtr = Marshal.StringToHGlobalAnsi("00dc");
            memcpy(&a.idx[4 * a.i_frame + 0], tmpPtr.ToPointer(), 4);
            Marshal.FreeHGlobal(tmpPtr);
            avi_set_dw(&a.idx[4 * a.i_frame + 1], b_key != 0 ? (int)AVIIF_KEYFRAME : 0);
            avi_set_dw(&a.idx[4 * a.i_frame + 2], (int)i_pos);
            avi_set_dw(&a.idx[4 * a.i_frame + 3], v.i_data);

            a.i_frame++;
        }

        void avi_set_dw(void* _p, int dw)
        {
            byte* p = (byte*)_p;
            p[0] = (byte)((dw) & 0xff);
            p[1] = (byte)((dw >> 8) & 0xff);
            p[2] = (byte)((dw >> 16) & 0xff);
            p[3] = (byte)((dw >> 24) & 0xff);
        }

        void vbuf_reset(vbuf_t v)
        {
            v.i_data = 0;
        }

        int ParseNAL(nal_t nal, avi_t a, h264_t h, int* pb_slice)
        {
            int b_flush = 0;
            int b_start;

            h264_parser_parse(h, nal, &b_start);

            if (b_start != 0 && *pb_slice != 0)
            {
                b_flush = 1;
                *pb_slice = 0;
            }

            if (nal.i_type >= (int)nal_unit_type_e.NAL_SLICE && nal.i_type <= (int)nal_unit_type_e.NAL_SLICE_IDR)
                *pb_slice = 1;

            return b_flush;
        }

        void h264_parser_parse(h264_t h, nal_t nal, int* pb_nal_start)
        {
            bs_s s = new bs_s();
            *pb_nal_start = 0;


            if (nal.i_type == (int)nal_unit_type_e.NAL_SPS || nal.i_type == (int)nal_unit_type_e.NAL_PPS)
                *pb_nal_start = 1;

            bs_init(s, nal.p_payload, nal.i_payload);

            if (nal.i_type == (int)nal_unit_type_e.NAL_SPS)
            {
                uint i_tmp;
                i_tmp = bs_read(s, 8);
                bs_skip(s, 1 + 1 + 1 + 5 + 8);
                /* sps id */
                bs_read_ue(s);

                if (i_tmp >= 100)
                {
                    bs_read_ue(s); // chroma_format_idc
                    bs_read_ue(s); // bit_depth_luma_minus8
                    bs_read_ue(s); // bit_depth_chroma_minus8
                    bs_skip(s, 1); // qpprime_y_zero_transform_bypass_flag
                    if (bs_read(s, 1) > 0) // seq_scaling_matrix_present_flag
                    {
                        int i, j;
                        for (i = 0; i < 8; i++)
                        {
                            if (bs_read(s, 1) > 0) // seq_scaling_list_present_flag[i]
                            {
                                i_tmp = 8;
                                for (j = 0; j < (i < 6 ? 16 : 64); j++)
                                {
                                    i_tmp += (uint)bs_read_se(s);
                                    if (i_tmp == 0)
                                        break;
                                }
                            }
                        }
                    }
                }

                /* Skip i_log2_max_frame_num */
                h.i_log2_max_frame_num = bs_read_ue(s) + 4;
                /* Read poc_type */
                h.i_poc_type = bs_read_ue(s);
                if (h.i_poc_type == 0)
                {
                    h.i_log2_max_poc_lsb = bs_read_ue(s) + 4;
                }
                else if (h.i_poc_type == 1)
                {
                    int i_cycle;
                    /* skip b_delta_pic_order_always_zero */
                    bs_skip(s, 1);
                    /* skip i_offset_for_non_ref_pic */
                    bs_read_se(s);
                    /* skip i_offset_for_top_to_bottom_field */
                    bs_read_se(s);
                    /* read i_num_ref_frames_in_poc_cycle */
                    i_cycle = bs_read_ue(s);
                    if (i_cycle > 256) i_cycle = 256;
                    while (i_cycle > 0)
                    {
                        /* skip i_offset_for_ref_frame */
                        bs_read_se(s);
                    }
                }
                /* i_num_ref_frames */
                bs_read_ue(s);
                /* b_gaps_in_frame_num_value_allowed */
                bs_skip(s, 1);

                /* Read size */
                h.i_width = 16 * (bs_read_ue(s) + 1);
                h.i_height = 16 * (bs_read_ue(s) + 1);

                /* b_frame_mbs_only */
                i_tmp = bs_read(s, 1);
                if (i_tmp == 0)
                {
                    bs_skip(s, 1);
                }
                /* b_direct8x8_inference */
                bs_skip(s, 1);

                /* crop ? */
                i_tmp = bs_read(s, 1);
                if (i_tmp != 0)
                {
                    /* left */
                    h.i_width -= 2 * bs_read_ue(s);
                    /* right */
                    h.i_width -= 2 * bs_read_ue(s);
                    /* top */
                    h.i_height -= 2 * bs_read_ue(s);
                    /* bottom */
                    h.i_height -= 2 * bs_read_ue(s);
                }

                /* vui: ignored */
                /*2012.8.9*/
                i_tmp = bs_read(s, 1);
                if (i_tmp != 0)
                {
                    bs_skip(s, 4);
                    i_tmp = bs_read(s, 1);
                    if (i_tmp != 0)
                    {
                        h.i_tick = (int)bs_read(s, 32);
                        h.i_time_scale = (int)bs_read(s, 32);
                        h.vui_flag = true;
                    }
                }


            }
            else if (nal.i_type >= (int)nal_unit_type_e.NAL_SLICE && nal.i_type <= (int)nal_unit_type_e.NAL_SLICE_IDR)
            {
                int i_tmp;

                /* i_first_mb */
                bs_read_ue(s);
                /* picture type */
                switch (bs_read_ue(s))
                {
                    case 0:
                    case 5: /* P */
                    case 1:
                    case 6: /* B */
                    case 3:
                    case 8: /* SP */
                        h.b_key = 0;
                        break;
                    case 2:
                    case 7: /* I */
                    case 4:
                    case 9: /* SI */
                        h.b_key = (nal.i_type == (int)nal_unit_type_e.NAL_SLICE_IDR) ? 1 : 0;
                        break;
                }
                /* pps id */
                bs_read_ue(s);

                /* frame num */
                i_tmp = (int)bs_read(s, h.i_log2_max_frame_num);

                if (i_tmp != h.i_frame_num)
                    *pb_nal_start = 1;

                h.i_frame_num = i_tmp;

                if (nal.i_type == (int)nal_unit_type_e.NAL_SLICE_IDR)
                {
                    i_tmp = bs_read_ue(s);
                    if (h.i_nal_type == (int)nal_unit_type_e.NAL_SLICE_IDR && h.i_idr_pic_id != i_tmp)
                        *pb_nal_start = 1;

                    h.i_idr_pic_id = i_tmp;
                }

                if (h.i_poc_type == 0)
                {
                    i_tmp = (int)bs_read(s, h.i_log2_max_poc_lsb);
                    if (i_tmp != h.i_poc)
                        *pb_nal_start = 1;
                    h.i_poc = i_tmp;
                }
            }
            h.i_nal_type = nal.i_type;
            h.i_ref_idc = nal.i_ref_idc;
        }

        void vbuf_add(vbuf_t v, int i_data, void* p_data)
        {
            if (i_data + v.i_data >= v.i_data_max)
            {
                v.i_data_max += i_data;
                v.p_data = (byte*)Marshal.ReAllocHGlobal(new IntPtr(v.p_data), new IntPtr(v.i_data_max)).ToPointer();
            }

            memcpy(&v.p_data[v.i_data], p_data, (uint)i_data);

            v.i_data += i_data;
        }

        void avi_end(avi_t a)
        {
            a.i_movi_end = a.f.Position;

            /* write index */
            avi_write_idx(a);

            a.i_riff = a.f.Position;

            /* Fix header */
            a.f.Seek(0, SeekOrigin.Begin);
            avi_write_header(a);
        }

        void avi_write_idx(avi_t a)
        {
            avi_write_fourcc(a, "idx1");
            avi_write_uint32(a, (uint)a.i_frame * 16);

            byte[] buffer = new byte[(a.i_frame * 16) * 1];
            Marshal.Copy(new IntPtr(a.idx), buffer, 0, buffer.Length);
            a.f.Write(buffer, 0, buffer.Length);
        }

        static int i_ctrl_c = 0;
        static void SigIntHandler(int a)
        {
            i_ctrl_c = 1;
        }



        // 标准仿C memcpy函数
        public void* memcpy(void* dst, void* src, uint count)
        {
            System.Diagnostics.Debug.Assert(dst != null);
            System.Diagnostics.Debug.Assert(src != null);

            void* ret = dst;

            /*
            * copy from lower addresses to higher addresses
            */
            while (count-- > 0)
            {
                *(byte*)dst = *(byte*)src;
                dst = (byte*)dst + 1;
                src = (byte*)src + 1;
            }

            return (ret);
        }

        // 标准仿C memmove函数
        public void* memmove(void* dst, void* src, uint count)
        {
            System.Diagnostics.Debug.Assert(dst != null);
            System.Diagnostics.Debug.Assert(src != null);

            void* ret = dst;
            if (dst <= src || dst >= (byte*)src + count)
            {
                while (count-- > 0)
                {
                    *(byte*)dst = *(byte*)src;
                    dst = (byte*)dst + 1;
                    src = (byte*)src + 1;
                }
            }
            else
            {
                dst = (byte*)dst + count - 1;
                src = (byte*)src + count - 1;
                while (count-- > 0)
                {
                    *(byte*)dst = *(byte*)src;
                    dst = (byte*)dst - 1;
                    src = (byte*)src - 1;
                }
            }

            return ret;

            /*
            byte* str1 = (byte*)dst;
            byte* str2 = (byte*)src;
            if ((str2 > str1) && (str2 < str1 + count))
            {
                while (count > 0)
                {
                    *str1 = *str2;
                    str1++;
                    str2++;
                    count--;
                }
            }
            else
            {
                while (count-->0)
                {
                    *(str1 + count) = *(str2 + count);
                }
            }
            return dst;
            */
        }

        // 标准仿C memset函数
        public void* memset(void* s, int c, uint n)
        {
            byte* p = (byte*)s;

            while (n > 0)
            {
                *p++ = (byte)c;
                --n;
            }

            return s;
        }


        public void Dispose()
        {
            if (!string.IsNullOrEmpty(_fileName) && fout?.CanWrite == true)
                ToAvi_EndFileWrite();
            avi?.Dispose();
            nal?.Dispose();
            vb?.Dispose();
            fin?.Dispose();
            fout?.Dispose();
            if (data != null)
            {
                Marshal.FreeHGlobal(new IntPtr(data));
            }
        }
    }


    public class cfg_t
    {
        public string psz_fin;
        public string psz_fout;
        public float f_fps;
        public string fcc;
    }

    public unsafe class nal_t : IDisposable
    {
        public int i_ref_idc;  /* nal_priority_e */
        public int i_type;     /* nal_unit_type_e */
        public int i_payload; /* This data are raw pay0load */
        public byte* p_payload;

        public void Dispose()
        {
            if (p_payload != null)
                Marshal.FreeHGlobal(new IntPtr(p_payload));
        }
    }

    public unsafe class avi_t : IDisposable
    {
        public FileStream f;
        public float f_fps;
        public string fcc;
        public int i_width;
        public int i_height;
        public long i_movi;
        public long i_movi_end;
        public long i_riff;
        public int i_frame;
        public int i_idx_max;
        public uint* idx;

        public void Dispose()
        {
            if (idx != null)
            {
                Marshal.FreeHGlobal(new IntPtr(idx));
            }
        }
    }

    public class h264_t
    {
        public int i_width;
        public int i_height;

        public int i_nal_type;
        public int i_ref_idc;
        public int i_idr_pic_id;
        public int i_frame_num;
        public int i_poc;

        public int b_key;
        public int i_log2_max_frame_num;
        public int i_poc_type;
        public int i_log2_max_poc_lsb;
        public int i_tick;
        public int i_time_scale;
        public bool vui_flag;
    }

    public unsafe class vbuf_t : IDisposable
    {
        public int i_data;
        public int i_data_max;
        public byte* p_data;

        public void Dispose()
        {
            if (p_data != null)
            {
                Marshal.FreeHGlobal(new IntPtr(p_data));
            }
        }
    }

    public enum nal_unit_type_e
    {
        NAL_UNKNOWN = 0,
        NAL_SLICE = 1,
        NAL_SLICE_DPA = 2,
        NAL_SLICE_DPB = 3,
        NAL_SLICE_DPC = 4,
        NAL_SLICE_IDR = 5,    /* ref_idc != 0 */
        NAL_SEI = 6,    /* ref_idc == 0 */
        NAL_SPS = 7,
        NAL_PPS = 8
        /* ref_idc == 0 for 6,9,10,11,12 */
    }
}
