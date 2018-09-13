
using os;

using sys;

using traceback;

using wx;

using pygame;

using util;

using battle;

using yadodb;

using data;

using dice;

using effectmotion;

using @event;

using eventhandler;

using eventrelay;

using features;

using scenariodb;

using setting;

using skin;

using animation;

using thread;

using header;

using image;

using imageretouch;

using frame;

using deck;

using character;

using effectbooster;

using content;

using xmlcreater;

using bassplayer;

using binary;

using advlog;

using update;

using dialog;

using debug;

using sprite;

using argparser;

using System;

using System.Collections.Generic;

public static class @__init__ {
    
    public static object cwpy = null;
    
    public static object tempdir_init = "Data/Temp/Global";
    
    public static object tempdir = tempdir_init;
    
    public static object APP_VERSION = Tuple.Create(2, "3");
    
    public static object APP_NAME = "CardWirthPy";
    
    public static object MBCS = "mbcs";
    
    public static object MBCS = "ms932";
    
    public static object LIMIT_RECURSE = 10000;
    
    public static object SIZE_SCR = Tuple.Create(640, 480);
    
    public static object SIZE_GAME = Tuple.Create(632, 453);
    
    public static object SIZE_AREA = Tuple.Create(632, 420);
    
    public static object SIZE_CARDIMAGE = Tuple.Create(74, 94);
    
    public static object SIZE_BOOK = Tuple.Create(460, 280);
    
    public static object SIZE_BILL = Tuple.Create(400, 370);
    
    public static object RECT_STATUSBAR = Tuple.Create(0, 420, 632, 33);
    
    public static object SUPPORTED_WSN = Tuple.Create("", "1", "2");
    
    public static object SUPPORTED_SKIN = Tuple.Create("0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11");
    
    public static object SCALE_LIST = Tuple.Create(2, 4, 8, 16);
    
    public static object AREAS_SP = Tuple.Create(-1, -2, -3, -4, -5);
    
    public static object AREAS_TRADE = Tuple.Create(-1, -2, -5);
    
    public static object AREA_TRADE1 = -1;
    
    public static object AREA_TRADE2 = -2;
    
    public static object AREA_TRADE3 = -5;
    
    public static object AREA_BREAKUP = -3;
    
    public static object AREA_CAMP = -4;
    
    public static object POCKET_SKILL = 0;
    
    public static object POCKET_ITEM = 1;
    
    public static object POCKET_BEAST = 2;
    
    public static object IDX_TREEEND = -1;
    
    public static object M_IMG = 0;
    
    public static object M_MSC = 1;
    
    public static object M_SND = 2;
    
    public static object EXTS_IMG = Tuple.Create(".bmp", ".jpg", ".jpeg", ".png", ".gif", ".pcx", ".tif", ".xpm");
    
    public static object EXTS_MSC = Tuple.Create(".mid", ".midi", ".mp3", ".ogg");
    
    public static object EXTS_SND = Tuple.Create(".wav", ".wave", ".ogg");
    
    public static object HINT_MESSAGE = 0;
    
    public static object HINT_CARD = 1;
    
    public static object HINT_AREA = 2;
    
    public static object HINT_SCENARIO = 3;
    
    public static object DEFAULT_SOUNDFONT = "Data/SoundFont/005.6mg_Aspirin_Stereo_V1.2_Bank.sf2";
    
    public static object LTYPE_MESSAGE = 1;
    
    public static object LTYPE_BACKGROUND = 2;
    
    public static object LTYPE_MCARDS = 3;
    
    public static object LTYPE_PCARDS = 4;
    
    public static object LTYPE_FCARDS = 0;
    
    public static object LTYPE_SPMESSAGE = 1;
    
    public static object LTYPE_SPMCARDS = 3;
    
    public static object LAYER_SP_LAYER = 10000000000L;
    
    public static object LAYER_BACKGROUND = 0;
    
    public static object LAYER_SPBACKGROUND = 1879048192;
    
    public static object LAYER_MCARDS = 100;
    
    public static object LAYER_PCARDS = 200;
    
    public static object LAYER_MCARDS_120 = 300;
    
    public static object LAYER_FCARDS_T = 2147483647L;
    
    public static object LAYER_FCARDS = 1000;
    
    public static object LAYER_BATTLE_START = Tuple.Create(2147483647L, 2147483647L - 3, 2147483647L, 2147483647L);
    
    public static object LAYER_FRONT_INUSECARD = Tuple.Create(2147483647L, 2147483647L - 2, 2147483647L, 2147483647L);
    
    public static object LAYER_TARGET_ARROW = Tuple.Create(2147483647L, 2147483647L - 1, 2147483647L, 2147483647L);
    
    public static object LAYER_MESSAGE = Tuple.Create(1000, LTYPE_MESSAGE, 0, 0);
    
    public static object LAYER_SELECTIONBAR_1 = Tuple.Create(1000, LTYPE_MESSAGE, 1, 0);
    
    public static object LAYER_SELECTIONBAR_2 = Tuple.Create(1000, LTYPE_MESSAGE, 2, 0);
    
    public static object LAYER_SPMESSAGE = Tuple.Create(LAYER_SP_LAYER + 1000, LTYPE_SPMESSAGE, 0, 0);
    
    public static object LAYER_SPSELECTIONBAR_1 = Tuple.Create(LAYER_SP_LAYER + 1000, LTYPE_MESSAGE, 1, 0);
    
    public static object LAYER_SPSELECTIONBAR_2 = Tuple.Create(LAYER_SP_LAYER + 1000, LTYPE_MESSAGE, 2, 0);
    
    public static object LAYER_TRANSITION = Tuple.Create(2147483647L, 2147483647L, 2147483647L, 2147483647L);
    
    public static object LAYER_LOG_CURTAIN = Tuple.Create(2000, 0, 0, 0);
    
    public static object LAYER_LOG = Tuple.Create(2001, 0, 0, 0);
    
    public static object LAYER_LOG_BAR = Tuple.Create(2002, 0, 0, 0);
    
    public static object LAYER_LOG_PAGE = Tuple.Create(2003, 0, 0, 0);
    
    public static object LAYER_LOG_SCROLLBAR = Tuple.Create(2004, 0, 0, 0);
    
    public static object UP_SCR = 1;
    
    public static object UP_WIN = 1;
    
    public static object UP_WIN_M = 1;
    
    public static object RESCALE_QUALITY = wx.IMAGE_QUALITY_BILINEAR;
    
    public static object RESCALE_QUALITY = wx.IMAGE_QUALITY_HIGH;
    
    public static object LOG_SEPARATOR_LEN_LONG = 80;
    
    public static object LOG_SEPARATOR_LEN_MIDDLE = 60;
    
    public static object LOG_SEPARATOR_LEN_SHORT = 45;
    
    public static object _argparser = argparser.ArgParser(appname: APP_NAME, description: String.Format("%s %s\n\nオープンソースのCardWirthエンジン", APP_NAME, ".".join(map(a => str(a), APP_VERSION))));
    
    static @__init__() {
        _argparser.add_argument("-h", type: @bool, nargs: 0, help: "このメッセージを表示して終了します。", arg2: "--help");
        _argparser.add_argument("-debug", type: @bool, nargs: 0, help: "デバッグモードで起動します。");
        _argparser.add_argument("-yado", type: str, nargs: 1, @default: "", help: "起動と同時に<YADO>のパスにある宿を読み込みます。");
        _argparser.add_argument("-party", type: str, nargs: 1, @default: "", help: "起動と同時に<PARTY>のパスにあるパーティを読み込みます。\n" + "-yadoと同時に指定しなかった場合は無視されます。");
        _argparser.add_argument("-scenario", type: str, nargs: 1, @default: "", help: "起動と同時に<SCENARIO>のパスにあるシナリオを開始します。\n" + "-yado及び-partyと同時に指定しなかった場合は無視されます。");
        _argparser.print_help();
        sys.exit(0);
        OPTIONS.yado = OPTIONS.yado.decode(_encoding);
        OPTIONS.party = OPTIONS.party.decode(_encoding);
        OPTIONS.scenario = OPTIONS.scenario.decode(_encoding);
        SKIN_CONV_ARGS.append(arg);
        sys.argv.remove(arg);
        main();
    }
    
    public static object OPTIONS = _argparser.parse_args(sys.argv[1]);
    
    public static object _encoding = sys.getfilesystemencoding();
    
    public static object SKIN_CONV_ARGS = new List<object>();
    
    // numを実際の表示サイズに変換する。
    //     num: int or 座標(x,y) or 矩形(x,y,width,height)
    //          or pygame.Surface or pygame.Bitmap or pygame.Image
    //     
    public static object wins(object num) {
        return _s_impl(num, UP_WIN);
    }
    
    // numを描画サイズに変換する。
    //     num: int or 座標(x,y) or 矩形(x,y,width,height)
    //          or pygame.Surface or pygame.Bitmap or pygame.Image
    //     
    public static object s(object num) {
        return _s_impl(num, UP_SCR);
    }
    
    // numを描画サイズから表示サイズに変換する。
    //     num: int or 座標(x,y) or 矩形(x,y,width,height)
    //          or pygame.Surface or pygame.Bitmap or pygame.Image
    //     
    public static object scr2win_s(object num) {
        if (UP_WIN == UP_SCR) {
            return _s_impl(num, 1);
        } else {
            return _s_impl(num, float(UP_WIN) / UP_SCR);
        }
    }
    
    // numを表示サイズから描画サイズに変換する。
    //     num: int or 座標(x,y) or 矩形(x,y,width,height)
    //          or pygame.Surface or pygame.Bitmap or pygame.Image
    //     
    public static object win2scr_s(object num) {
        if (UP_WIN == UP_SCR) {
            return _s_impl(num, 1);
        } else {
            return _s_impl(num, float(UP_SCR) / UP_WIN);
        }
    }
    
    // numを描画サイズから表示サイズに変換する。
    //     num: int or 座標(x,y) or 矩形(x,y,width,height)
    //          or pygame.Surface or pygame.Bitmap or pygame.Image
    //     
    public static object scr2mwin_s(object num) {
        if (UP_WIN_M == UP_SCR) {
            return _s_impl(num, 1);
        } else {
            return _s_impl(num, float(UP_WIN_M) / UP_SCR);
        }
    }
    
    // numを表示サイズから描画サイズに変換する。
    //     num: int or 座標(x,y) or 矩形(x,y,width,height)
    //          or pygame.Surface or pygame.Bitmap or pygame.Image
    //     
    public static object mwin2scr_s(object num) {
        if (UP_WIN_M == UP_SCR) {
            return _s_impl(num, 1);
        } else {
            return _s_impl(num, float(UP_SCR) / UP_WIN_M);
        }
    }
    
    public static object _s_impl(object num, object up_scr) {
        object maskcolour;
        object bmpdepthis1;
        object img;
        object result;
        object size;
        object scr_scale;
        object bmp;
        object scaleinfo;
        object h;
        object w;
        object y;
        object x;
        if (num is tuple && num.Count == 3 && num[2] == null) {
            // スケール情報無し
            return _s_impl(num[::2], up_scr);
        }
        if (up_scr == 1 && !(num is tuple && num.Count == 3)) {
            // 拡大率が1倍で、スケール情報も無い
            if (num is tuple && num.Count == 2) {
                if (num[0] is pygame.Surface || num[0] is wx.Bitmap || num[0] is wx.Image) {
                    // 画像はそのままのサイズで表示
                    return num[0];
                }
            }
            // 座標等はそのまま返す
            return num;
        }
        if (num is int || num is float) {
            // 単純な数値(座標やサイズ)
            return Convert.ToInt32(num * up_scr);
        } else if (num is pygame.Rect) {
            // pygameの矩形情報
            if (num.Count == 4) {
                x = Convert.ToInt32(num[0] * up_scr);
                y = Convert.ToInt32(num[1] * up_scr);
                w = Convert.ToInt32(num[2] * up_scr);
                h = Convert.ToInt32(num[3] * up_scr);
                return pygame.Rect(x, y, w, h);
            }
        } else if (num is tuple) {
            if (num.Count == 3) {
                Console.WriteLine("A scaleinfo is deprecated.");
                traceback.print_stack();
                scaleinfo = num[2];
            } else {
                scaleinfo = null;
            }
            if (num[0] is pygame.Surface) {
                bmp = num[0];
                if (bmp.get_width() <= 0 || bmp.get_width() <= 0) {
                    return bmp;
                }
                if (scaleinfo) {
                    // スケール情報のあるpygame.Surface
                    // TODO scaleinfoは廃止
                    scr_scale = hasattr(bmp, "scr_scale") ? bmp.scr_scale : 1;
                    up_scr /= scr_scale;
                    if (up_scr == 1) {
                        return bmp;
                    }
                    size = _s_impl(num[1], up_scr);
                    if (size[0] % num[1] == 0) {
                        result = pygame.transform.scale(bmp, size);
                    } else {
                        if (!(bmp.get_flags() & pygame.locals.SRCALPHA) && bmp.get_colorkey()) {
                            bmp = bmp.convert_alpha();
                        }
                        result = image.smoothscale(bmp, size);
                    }
                    if (num[0] is util.Depth1Surface) {
                        result = util.Depth1Surface(result, scr_scale);
                    }
                    return result;
                } else {
                    // スケール情報の無いpygame.Surface(単純拡大)
                    return _s_impl(bmp, up_scr);
                }
            } else if (num[0] is wx.Image) {
                img = num[0];
                if (img.GetWidth() <= 0 || img.GetHeight() <= 0) {
                    return img;
                }
                if (scaleinfo) {
                    // スケール情報のあるwx.Image
                    // TODO scaleinfoは廃止
                    bmpdepthis1 = hasattr(img, "bmpdepthis1");
                    maskcolour = hasattr(img, "maskcolour") ? img.maskcolour : null;
                    scr_scale = hasattr(img, "scr_scale") ? img.scr_scale : 1;
                    up_scr /= scr_scale;
                    if (up_scr == 1) {
                        return img;
                    }
                    size = _s_impl(num[1], up_scr);
                    if (size[0] % num[1] == 0 || bmpdepthis1) {
                        result = img.Rescale(size[0], size[1], wx.IMAGE_QUALITY_NORMAL);
                    } else {
                        if (!img.HasAlpha()) {
                            img.InitAlpha();
                        }
                        result = img.Rescale(size[0], size[1], RESCALE_QUALITY);
                    }
                    if (bmpdepthis1) {
                        result.bmpdepthis1 = bmpdepthis1;
                    }
                    if (maskcolour) {
                        result.maskcolour = maskcolour;
                    }
                    return result;
                } else {
                    // スケール情報の無いwx.Image(単純拡大)
                    return _s_impl(img, up_scr);
                }
            } else if (num[0] is wx.Bitmap) {
                bmp = num[0];
                bmpdepthis1 = hasattr(bmp, "bmpdepthis1");
                maskcolour = hasattr(bmp, "maskcolour") ? bmp.maskcolour : null;
                scr_scale = hasattr(bmp, "scr_scale") ? bmp.scr_scale : 1;
                up_scr /= scr_scale;
                if (up_scr == 1) {
                    return bmp;
                }
                if (bmp.GetWidth() <= 0 || bmp.GetHeight() <= 0) {
                    return bmp;
                }
                // wx.Bitmap
                if (bmpdepthis1) {
                    img = util.convert_to_image(bmp);
                } else {
                    img = bmp.ConvertToImage();
                }
                result = _s_impl(Tuple.Create(img, num[1]), up_scr).ConvertToBitmap();
                if (bmpdepthis1) {
                    result.bmpdepthis1 = bmpdepthis1;
                }
                if (maskcolour) {
                    result.maskcolour = maskcolour;
                }
                return result;
            } else if (num.Count == 4) {
                // 矩形
                x = Convert.ToInt32(num[0] * up_scr);
                y = Convert.ToInt32(num[1] * up_scr);
                w = Convert.ToInt32(num[2] * up_scr);
                h = Convert.ToInt32(num[3] * up_scr);
                return Tuple.Create(x, y, w, h);
            } else if (num.Count == 2) {
                // 座標
                x = Convert.ToInt32(num[0] * up_scr);
                y = Convert.ToInt32(num[1] * up_scr);
                return Tuple.Create(x, y);
            }
        } else if (num is pygame.Surface) {
            // スケール情報の無いpygame.Surface(単純拡大)
            var bmp0 = num;
            scr_scale = hasattr(num, "scr_scale") ? num.scr_scale : 1;
            up_scr /= scr_scale;
            if (up_scr == 1) {
                return num;
            }
            w = Convert.ToInt32(num.get_width() * up_scr);
            h = Convert.ToInt32(num.get_height() * up_scr);
            if (w <= 0 || h <= 0) {
                return num;
            }
            size = Tuple.Create(w, h);
            if (up_scr % 1 == 0) {
                result = pygame.transform.scale(num, size);
            } else {
                if (!(num.get_flags() & pygame.locals.SRCALPHA) && num.get_colorkey()) {
                    num = num.convert_alpha();
                }
                result = image.smoothscale(num, size);
            }
            if (bmp0 is util.Depth1Surface) {
                result = util.Depth1Surface(result, scr_scale);
            }
            return result;
        } else if (num is wx.Image) {
            // スケール情報の無いwx.Image(単純拡大)
            bmpdepthis1 = hasattr(num, "bmpdepthis1");
            maskcolour = hasattr(num, "maskcolour") ? num.maskcolour : null;
            scr_scale = hasattr(num, "scr_scale") ? num.scr_scale : 1;
            up_scr /= scr_scale;
            if (up_scr == 1) {
                return num;
            }
            w = Convert.ToInt32(num.GetWidth() * up_scr);
            h = Convert.ToInt32(num.GetHeight() * up_scr);
            if (w <= 0 || h <= 0) {
                return num;
            }
            if (up_scr % 1 == 0 || bmpdepthis1) {
                result = num.Rescale(w, h, wx.IMAGE_QUALITY_NORMAL);
            } else {
                if (!num.HasAlpha()) {
                    num.InitAlpha();
                }
                result = num.Rescale(w, h, RESCALE_QUALITY);
            }
            if (bmpdepthis1) {
                result.bmpdepthis1 = bmpdepthis1;
            }
            if (maskcolour) {
                result.maskcolour = maskcolour;
            }
            return result;
        } else if (num is wx.Bitmap) {
            // スケール情報の無いwx.Bitmap(単純拡大)
            bmpdepthis1 = hasattr(num, "bmpdepthis1");
            maskcolour = hasattr(num, "maskcolour") ? num.maskcolour : null;
            scr_scale = hasattr(num, "scr_scale") ? num.scr_scale : 1;
            up_scr /= scr_scale;
            if (up_scr == 1) {
                return num;
            }
            w = Convert.ToInt32(num.GetWidth() * up_scr);
            h = Convert.ToInt32(num.GetHeight() * up_scr);
            if (w <= 0 || h <= 0) {
                return num;
            }
            bmp = num;
            if (bmpdepthis1) {
                img = util.convert_to_image(bmp);
            } else {
                img = bmp.ConvertToImage();
            }
            img = _s_impl(img, up_scr);
            result = img.ConvertToBitmap();
            if (bmpdepthis1) {
                result.bmpdepthis1 = bmpdepthis1;
            }
            if (maskcolour) {
                result.maskcolour = maskcolour;
            }
            return result;
        }
        return num;
    }
    
    public static object dpi_level = 1;
    
    public static object ppis(object num) {
        return _s_impl(num, dpi_level);
    }
    
    public static object main() {
    }
}
