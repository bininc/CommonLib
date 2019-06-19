using bs_t = CommonLib.WinAPI.Video.bs.bs_s;

namespace CommonLib.WinAPI.Video
{
    public unsafe class bs
    {
        //struct bs_s bs_t;
        public class bs_s
        {
            public byte* p_start;
            public byte* p;
            public byte* p_end;

            public int i_left;    /* i_count number of available bits */
            public int i_bits_encoded; /* RD only */
        };

        public static void bs_init(bs_t s, void* p_data, int i_data)
        {
            s.p_start = (byte*)p_data;
            s.p = (byte*)p_data;
            s.p_end = s.p + i_data;
            s.i_left = 8;
        }
        public static int bs_pos(bs_t s)
        {
            return (int)(8 * (s.p - s.p_start) + 8 - s.i_left);
        }
        public static int bs_eof(bs_t s)
        {
            return (s.p >= s.p_end ? 1 : 0);
        }

        static uint[] i_mask =  { 0x00,
                                  0x01,      0x03,      0x07,      0x0f,
                                  0x1f,      0x3f,      0x7f,      0xff,
                                  0x1ff,     0x3ff,     0x7ff,     0xfff,
                                  0x1fff,    0x3fff,    0x7fff,    0xffff,
                                  0x1ffff,   0x3ffff,   0x7ffff,   0xfffff,
                                  0x1fffff,  0x3fffff,  0x7fffff,  0xffffff,
                                  0x1ffffff, 0x3ffffff, 0x7ffffff, 0xfffffff,
                                  0x1fffffff,0x3fffffff,0x7fffffff,0xffffffff};

        public static uint bs_read(bs_t s, int i_count)
        {           
            int i_shr;
            uint i_result = 0;

            while (i_count > 0)
            {
                if (s.p >= s.p_end)
                {
                    break;
                }

                if ((i_shr = s.i_left - i_count) >= 0)
                {
                    /* more in the buffer than requested */
                    i_result |= (uint)((*s.p >> i_shr) & i_mask[i_count]);
                    s.i_left -= i_count;
                    if (s.i_left == 0)
                    {
                        s.p++;
                        s.i_left = 8;
                    }
                    return (i_result);
                }
                else
                {
                    /* less in the buffer than requested */
                    i_result |= (*s.p & i_mask[s.i_left]) << -i_shr;
                    i_count -= s.i_left;
                    s.p++;
                    s.i_left = 8;
                }
            }

            return (i_result);
        }

        static uint bs_read1(bs_t s)
        {

            if (s.p < s.p_end)
            {
                uint i_result;

                s.i_left--;
                i_result = (uint)((*s.p >> s.i_left) & 0x01);
                if (s.i_left == 0)
                {
                    s.p++;
                    s.i_left = 8;
                }
                return i_result;
            }

            return 0;
        }
        static uint bs_show(bs_t s, int i_count)
        {
            if (s.p < s.p_end && i_count > 0)
            {
                uint i_cache = (uint)(((s.p[0] << 24) + (s.p[1] << 16) + (s.p[2] << 8) + s.p[3]) << (8 - s.i_left));
                return (i_cache >> (32 - i_count));
            }
            return 0;
        }

        /* TODO optimize */
        public static void bs_skip(bs_t s, int i_count)
        {
            s.i_left -= i_count;

            while (s.i_left <= 0)
            {
                s.p++;
                s.i_left += 8;
            }
        }


        public static int bs_read_ue(bs_t s)
        {
            int i = 0;

            while (bs_read1(s) == 0 && s.p < s.p_end && i < 32)
            {
                i++;
            }
            return (int)((1 << i) - 1 + bs_read(s, i));
        }
        public static int bs_read_se(bs_t s)
        {
            int val = bs_read_ue(s);

            return (val & 0x01) != 0 ? (val + 1) / 2 : -(val / 2);
        }

        static int bs_read_te(bs_t s, int x)
        {
            if (x == 1)
            {
                return (int)(1 - bs_read1(s));
            }
            else if (x > 1)
            {
                return bs_read_ue(s);
            }
            return 0;
        }

        static void bs_write(bs_t s, int i_count, uint i_bits)
        {
            if (s.p >= s.p_end - 4)
                return;
            while (i_count > 0)
            {
                if (i_count < 32)
                    i_bits &= (uint)((1 << i_count) - 1);
                if (i_count < s.i_left)
                {
                    *s.p = (byte)((*s.p << i_count) | i_bits);
                    s.i_left -= i_count;
                    break;
                }
                else
                {
                    *s.p = (byte)((*s.p << s.i_left) | (i_bits >> (i_count - s.i_left)));
                    i_count -= s.i_left;
                    s.p++;
                    s.i_left = 8;
                }
            }
        }

        static void bs_write1(bs_t s, uint i_bit)
        {
            if (s.p < s.p_end)
            {
                *s.p <<= 1;
                *s.p |= (byte)i_bit;
                s.i_left--;
                if (s.i_left == 0)
                {
                    s.p++;
                    s.i_left = 8;
                }
            }
        }

        static void bs_align_0(bs_t s)
        {
            if (s.i_left != 8)
            {
                *s.p <<= s.i_left;
                s.i_left = 8;
                s.p++;
            }
        }
        static void bs_align_1(bs_t s)
        {
            if (s.i_left != 8)
            {
                *s.p <<= s.i_left;
                *s.p |= (byte)((1 << s.i_left) - 1);
                s.i_left = 8;
                s.p++;
            }
        }
        static void bs_align(bs_t s)
        {
            bs_align_0(s);
        }

        static int[] i_size0_255 =
        {
            1,1,2,2,3,3,3,3,4,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
            6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
            7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
            7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
            8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
            8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
            8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
            8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8
        };

        /* golomb functions */

        static void bs_write_ue(bs_t s, uint val)
        {
            int i_size = 0;


            if (val == 0)
            {
                bs_write1(s, 1);
            }
            else
            {
                uint tmp = ++val;

                if (tmp >= 0x00010000)
                {
                    i_size += 16;
                    tmp >>= 16;
                }
                if (tmp >= 0x100)
                {
                    i_size += 8;
                    tmp >>= 8;
                }
                i_size += i_size0_255[tmp];

                bs_write(s, 2 * i_size - 1, val);
            }
        }

        static void bs_write_se(bs_t s, int val)
        {
            bs_write_ue(s, (uint)(val <= 0 ? -val * 2 : val * 2 - 1));
        }

        static void bs_write_te(bs_t s, int x, int val)
        {
            if (x == 1)
            {
                bs_write1(s, (uint)(1 & ~val));
            }
            else if (x > 1)
            {
                bs_write_ue(s, (uint)val);
            }
        }

        static void bs_rbsp_trailing(bs_t s)
        {
            bs_write1(s, 1);
            if (s.i_left != 8)
            {
                bs_write(s, s.i_left, 0x00);
            }
        }

        static int[] i_size0_254 =
        {
            1, 3, 3, 5, 5, 5, 5, 7, 7, 7, 7, 7, 7, 7, 7,
            9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
            11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,
            11,11,11,11,11,11,11,11,11,13,13,13,13,13,13,13,13,13,13,13,13,13,13,
            13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,
            13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,
            13,13,13,13,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,
            15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15
        };

        static int bs_size_ue(uint val)
        {
            if (val < 255)
            {
                return i_size0_254[val];
            }
            else
            {
                int i_size = 0;

                val++;

                if (val >= 0x10000)
                {
                    i_size += 32;
                    val = (val >> 16) - 1;
                }
                if (val >= 0x100)
                {
                    i_size += 16;
                    val = (val >> 8) - 1;
                }
                return i_size0_254[val] + i_size;
            }
        }

        static int bs_size_se(int val)
        {
            return bs_size_ue((uint)(val <= 0 ? -val * 2 : val * 2 - 1));
        }

        static int bs_size_te(int x, int val)
        {
            if (x == 1)
            {
                return 1;
            }
            else if (x > 1)
            {
                return bs_size_ue((uint)val);
            }
            return 0;
        }
    }
}
