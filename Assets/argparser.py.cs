
using sys;

using System.Collections.Generic;

using System;

public static class argparser {
    
    public class ArgParser
        : object {
        
        public ArgParser(object appname = "", object description = "") {
            this.appname = appname;
            this.desc = description;
            this.args = new Dictionary<object, object> {
            };
            this.largs = new List<object>();
        }
        
        // オプションの情報を追加する。
        //         arg: '-'で始まるオプション名。
        //         type: オプションの型。str, int, boolのいずれか。
        //         nargs: オプションが取る引数の数。常に数値で指定する。
        //         help: オプションの解説。
        //         arg2: '--'で始まるオプション名。
        //         default: オプションのデフォルト値。
        //         
        public virtual object add_argument(
            object arg,
            object type,
            object nargs,
            object help,
            object arg2 = "",
            object @default = null) {
            var argobj = Arg(arg, type, nargs, help, arg2, @default);
            this.args[arg] = argobj;
            if (arg2) {
                this.args[arg2] = argobj;
            }
            this.largs.append(argobj);
        }
        
        // 引数をパースした結果を得る。
        //         args: 引数のリスト。未指定の場合はsys.argvを使用する。
        //         
        public virtual object parse_args(object args = null) {
            object argobj;
            if (args == null) {
                args = sys.argv[1];
            } else {
                args = args[":"];
            }
            var r = ArgResult();
            var keys = this.args.keys();
            try {
                while (args) {
                    var arg = args.pop(0);
                    if (this.args.Contains(arg)) {
                        argobj = this.args[arg];
                        var val = argobj.eat(args);
                        setattr(r, argobj.arg[1], val);
                        keys.remove(arg);
                        if (argobj.arg2) {
                            setattr(r, argobj.arg2[2], val);
                            keys.remove(argobj.arg2);
                        }
                    } else {
                        r.leftovers.append(arg);
                    }
                }
            } catch {
                Console.WriteLine(String.Format("起動引数が正しくありません: %s", arg));
                Console.WriteLine();
                this.print_help();
                return null;
            }
            foreach (var key in keys) {
                argobj = this.args[key];
                setattr(r, argobj.arg[1], argobj.@default);
                if (argobj.arg2) {
                    setattr(r, argobj.arg2[2], argobj.@default);
                }
            }
            return r;
        }
        
        // ヘルプメッセージを表示する。
        //         
        public virtual object print_help() {
            object help;
            var s = new List<object> {
                "Usage:",
                this.appname
            };
            foreach (var arg in this.largs) {
                help = arg.get_help("|");
                s.append(String.Format("[%s]", help));
            }
            Console.WriteLine(" ".join(s));
            Console.WriteLine();
            Console.WriteLine(this.desc);
            Console.WriteLine();
            Console.WriteLine("オプション:");
            var mlen = 0;
            foreach (var arg in this.largs) {
                help = arg.get_help();
                mlen = max(help.Count, mlen);
            }
            foreach (var arg in this.largs) {
                s = arg.get_help();
                s = s.ljust(mlen);
                Console.WriteLine(String.Format("  %s  %s", s, ("\n" + " " * (mlen + 4)).join(arg.help.splitlines())));
            }
        }
    }
    
    public class ArgResult
        : object {
        
        public ArgResult() {
            this.leftovers = new List<object>();
        }
    }
    
    public class Arg
        : object {
        
        public Arg(
            object arg,
            object type,
            object nargs,
            object help,
            object arg2 = "",
            object @default = null) {
            this.arg = arg;
            this.arg2 = arg2;
            this.type = type;
            this.nargs = nargs;
            this.help = help;
            this.@default = @default;
        }
        
        // argsからオプション引数を得る。
        //         argsの要素は、得られた引数の分だけ
        //         前方から除去される。
        //         
        public virtual object eat(object args) {
            if (this.nargs == 1) {
                return this.parse(args.pop(0));
            } else if (1 < this.nargs) {
                var seq = new List<object>();
                foreach (var _i in xrange(this.nargs.Count)) {
                    seq.append(this.parse(args.pop(0)));
                }
                return seq;
            } else {
                return true;
            }
        }
        
        // 型に応じて引数をパースする。
        public virtual object parse(object value) {
            if (this.type == @int) {
                return Convert.ToInt32(value);
            } else if (this.type == str) {
                return value;
            }
        }
        
        // ヘルプメッセージ用のテキストを生成する。
        //         
        public virtual object get_help(object sep = ", ") {
            var s = this.arg;
            if (this.arg2) {
                s = String.Format("%s%s%s", s, sep, this.arg2);
            }
            if (this.nargs) {
                return String.Format("%s <%s>", s, this.arg[1].upper());
            } else {
                return s;
            }
        }
    }
    
    //!/usr/bin/env python
    // -*- coding: utf-8 -*-
    public static object main() {
        var parser = ArgParser(appname: "args.py", description: "Process some integers.");
        parser.add_argument("-h", type: @bool, nargs: 0, help: "このメッセージを表示して終了します。", arg2: "--help", @default: false);
        parser.add_argument("-y", type: str, nargs: 1, help: "help1\nhelp2", @default: "bbb");
        parser.add_argument("-dbg", type: str, nargs: 0, help: "help");
        var args = parser.parse_args();
        if (!args) {
            sys.exit(-1);
        }
        if (args.help) {
            parser.print_help();
            return;
        }
        Console.WriteLine("-y  :", args.y);
        Console.WriteLine("-dbg:", args.dbg);
        Console.WriteLine("    :", args.leftovers);
    }
    
    static argparser() {
        main();
    }
}
