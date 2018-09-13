
using os;

using io;

using sys;

using time;

using StringIO;

using sqlite3;

using threading;

using subprocess;

using cw;

using synclock = cw.util.synclock;

using System.Collections.Generic;

using System;

public static class scenariodb {
    
    public static object _lock = threading.Lock();
    
    public static object TYPE_WSN = 0;
    
    public static object TYPE_CLASSIC = 1;
    
    public static object DATA_TITLE = 0;
    
    public static object DATA_DESC = 1;
    
    public static object DATA_AUTHOR = 2;
    
    public static object DATA_LEVEL = 3;
    
    public static object DATA_FNAME = 4;
    
    public class ScenariodbUpdatingThread
        : threading.Thread {
        
        public object _finished = false;
        
        public ScenariodbUpdatingThread(object setting, object vacuum = false, object dpath = "Scenario", object skintype = "") {
            this.setting = setting;
            this._vacuum = vacuum;
            this._dpath = dpath;
            this._skintype = skintype;
        }
        
        public virtual object run() {
            type(this)._finished = false;
            var db = Scenariodb();
            db.update(skintype: this._skintype);
            var folders = new HashSet<object>();
            folders.add(this._dpath);
            foreach (var _tup_1 in this.setting.folderoftype) {
                var _skintype = _tup_1.Item1;
                var folder = _tup_1.Item2;
                if (!folders.Contains(folder)) {
                    db.update(folder, skintype: this._skintype);
                    folders.add(folder);
                }
            }
            if (this._vacuum) {
                db.vacuum();
            }
            db.close();
            type(this)._finished = true;
        }
        
        [staticmethod]
        public static object is_finished() {
            return ScenariodbUpdatingThread._finished;
        }
    }
    
    // シナリオデータベース。ロックのタイムアウトは30秒指定。
    //     データ種類は、
    //     dpath(ファイルのあるディレクトリ),
    //     type(シナリオのタイプ。0=wsn, 1=クラシック),
    //     fname(wsnファイル名、またはフォルダ名),
    //     name(シナリオ名),
    //     author(作者),
    //     desc(解説文),
    //     skintype(スキン種類),
    //     levelmin(最低対象レベル),
    //     levelmax(最高対象レベル),
    //     coupons(必須クーポン。"\n"が区切り),
    //     couponsnum(必須クーポン数),
    //     startid(開始エリアID),
    //     tags(タグ。"\n"が区切り),
    //     ctime(DB登録時間。エポック秒),
    //     mtime(ファイル最終更新時間。エポック秒),
    //     image(見出し画像のイメージデータ),
    //     imgpath(見出し画像のパス),
    //     wsnversion(WSN形式の場合はそのバージョン)
    //     
    public class Scenariodb
        : object {
        
        [synclock(_lock)]
        public Scenariodb() {
            object s;
            this.name = "Scenario.db";
            if (os.path.isfile(this.name)) {
                this.con = sqlite3.connect(this.name, timeout: 30000);
                this.con.row_factory = sqlite3.Row;
                this.cur = this.con.cursor();
                var needcommit = false;
                // type, imgpath列が存在しない場合は作成する(旧バージョンとの互換性維持)
                var cur = this.con.execute("PRAGMA table_info('scenariodb')");
                var res = cur.fetchall();
                var hastype = false;
                var hasimgpath = false;
                var haswsnversion = false;
                foreach (var rec in res) {
                    if (rec[1] == "type") {
                        hastype = true;
                    } else if (rec[1] == "imgpath") {
                        hasimgpath = true;
                    } else if (rec[1] == "wsnversion") {
                        haswsnversion = true;
                    }
                    if (all(Tuple.Create(hastype, hasimgpath, haswsnversion))) {
                        break;
                    }
                }
                if (!hastype) {
                    this.cur.execute("ALTER TABLE scenariodb ADD COLUMN type INTEGER");
                    this.cur.execute("UPDATE scenariodb SET type=?", Tuple.Create(TYPE_WSN));
                    needcommit = true;
                }
                if (!hasimgpath) {
                    // 値はNoneのままにしておく
                    this.cur.execute("ALTER TABLE scenariodb ADD COLUMN imgpath TEXT");
                    needcommit = true;
                }
                if (!haswsnversion) {
                    // 値はNoneのままにしておく
                    this.cur.execute("ALTER TABLE scenariodb ADD COLUMN wsnversion TEXT");
                    needcommit = true;
                }
                cur = this.con.execute("PRAGMA index_info('scenariodb_index1')");
                res = cur.fetchall();
                if (!res.Count) {
                    this.cur.execute("CREATE INDEX scenariodb_index1 ON scenariodb(dpath)");
                    needcommit = true;
                }
                cur = this.con.execute("PRAGMA table_info('scenariotype')");
                res = cur.fetchall();
                if (!res) {
                    s = @"
                    CREATE TABLE scenariotype (
                        dpath TEXT,
                        fname TEXT,
                        skintype TEXT,
                        PRIMARY KEY (dpath, fname, skintype)
                    )
                ";
                    this.cur.execute(s);
                    needcommit = true;
                }
                //# FIXME: SQLite3ではDROP COLUMNは使用できないので放置しておく
                //## imgpathが存在する場合は削除する(0.12.4αで一旦必要になったものの不要化)
                //#cur = self.con.execute("PRAGMA table_info('scenariodb')")
                //#res = cur.fetchall()
                //#hastype = False
                //#for rec in res:
                //#    if rec[1] == "imgpath":
                //#        hastype = True
                //#        break
                //#  if hastype:
                //#    self.cur.execute("ALTER TABLE scenariodb DROP COLUMN imgpath")
                //#    needcommit = True
                // scenarioimageテーブルが存在しない場合は作成する(0.12.3以前との互換性維持)
                cur = this.con.execute("PRAGMA table_info('scenarioimage')");
                res = cur.fetchall();
                if (!res) {
                    s = @"
                    CREATE TABLE scenarioimage (
                        dpath TEXT,
                        fname TEXT,
                        numorder INTEGER,
                        scale INTEGER,
                        image BLOB,
                        imgpath TEXT,
                        postype TEXT,
                        PRIMARY KEY (dpath, fname, numorder, scale)
                    )
                ";
                    this.cur.execute(s);
                } else {
                    // postype, imgpath, scale列が存在しない場合は作成する(～1.1との互換性維持)
                    cur = this.con.execute("PRAGMA table_info('scenarioimage')");
                    res = cur.fetchall();
                    var haspostype = false;
                    hasimgpath = false;
                    var hasscale = false;
                    foreach (var rec in res) {
                        if (rec[1] == "postype") {
                            haspostype = true;
                        } else if (rec[1] == "imgpath") {
                            hasimgpath = true;
                        } else if (rec[1] == "scale") {
                            hasscale = true;
                        }
                        if (all(Tuple.Create(haspostype, hasimgpath, hasscale))) {
                            break;
                        }
                    }
                    if (!haspostype) {
                        // 値はNone(Default扱い)
                        this.cur.execute("ALTER TABLE scenarioimage ADD COLUMN postype TEXT");
                        needcommit = true;
                    }
                    if (!hasimgpath) {
                        // 値はNoneのままにしておく
                        this.cur.execute("ALTER TABLE scenarioimage ADD COLUMN imgpath TEXT");
                        needcommit = true;
                    }
                    if (!hasscale) {
                        // SQLite3では主キーの変更ができないので作り直す
                        s = @"
                        CREATE TABLE scenarioimage_temp (
                            dpath TEXT,
                            fname TEXT,
                            numorder INTEGER,
                            scale INTEGER,
                            image BLOB,
                            imgpath TEXT,
                            postype TEXT,
                            PRIMARY KEY (dpath, fname, numorder, scale)
                        )
                    ";
                        this.cur.execute(s);
                        s = @"
                        INSERT INTO scenarioimage_temp (
                            dpath,
                            fname,
                            numorder,
                            scale,
                            image,
                            imgpath,
                            postype
                        )
                        SELECT
                            dpath,
                            fname,
                            numorder,
                            1,
                            image,
                            imgpath,
                            postype
                        FROM
                            scenarioimage
                    ";
                        this.cur.execute(s);
                        s = "DROP TABLE scenarioimage";
                        this.cur.execute(s);
                        s = "ALTER TABLE scenarioimage_temp RENAME TO scenarioimage";
                        this.cur.execute(s);
                        needcommit = true;
                    }
                }
                if (needcommit) {
                    this.con.commit();
                }
            } else {
                this.con = sqlite3.connect(this.name, timeout: 30000);
                this.cur = this.con.cursor();
                // テーブル作成
                s = @"CREATE TABLE scenariodb (
                   dpath TEXT, type INTEGER, fname TEXT, name TEXT, author TEXT,
                   desc TEXT, skintype TEXT, levelmin INTEGER, levelmax INTEGER,
                   coupons TEXT, couponsnum INTEGER, startid INTEGER,
                   tags TEXT, ctime INTEGER, mtime INTEGER, image BLOB,
                   imgpath TEXT, wsnversion TEXT,
                   PRIMARY KEY (dpath, fname))";
                this.cur.execute(s);
                this.cur.execute("CREATE INDEX scenariodb_index1 ON scenariodb(dpath)");
                s = @"
                CREATE TABLE scenarioimage (
                    dpath TEXT,
                    fname TEXT,
                    numorder INTEGER,
                    scale INTEGER,
                    image BLOB,
                    imgpath TEXT,
                    postype TEXT,
                    PRIMARY KEY (dpath, fname, numorder)
                )
            ";
                this.cur.execute(s);
                s = @"
                CREATE TABLE scenariotype (
                    dpath TEXT,
                    fname TEXT,
                    skintype TEXT,
                    PRIMARY KEY (dpath, fname, skintype)
                )
            ";
                this.cur.execute(s);
                this.cur.execute("CREATE INDEX scenariotype_index1 ON scenariodb(dpath, fname)");
            }
        }
        
        // データベースを更新する。
        [synclock(_lock)]
        public virtual object update(object dpath = "Scenario", object skintype = "", object commit = true, object update = true) {
            object s;
            if (!update) {
                return;
            }
            if (skintype) {
                s = "SELECT A.dpath, A.fname, mtime, B.skintype FROM scenariodb A LEFT JOIN scenariotype B" + " ON A.dpath=B.dpath AND A.fname=B.fname" + " WHERE A.dpath=? AND (B.skintype=? OR B.skintype IS NULL)";
                this.cur.execute(s, Tuple.Create(cw.util.get_linktarget(dpath), skintype));
            } else {
                s = "SELECT dpath, fname, mtime FROM scenariodb WHERE dpath=?";
                this.cur.execute(s, Tuple.Create(cw.util.get_linktarget(dpath)));
            }
            var data = this.cur.fetchall();
            var dbpaths = new List<object>();
            Func<object, object, object, object> update_path = (t,spath,path) => {
                if (os.path.getmtime(spath) > t[2]) {
                    // 情報を更新
                    this._insert_scenario(path, false, skintype: skintype);
                } else if (skintype && t[3] == null) {
                    // タイプ情報がないので収集
                    this._insert_scenario(path, false, skintype: skintype);
                }
            };
            foreach (var t in data) {
                var path = "/".join(Tuple.Create(t[0], t[1]));
                var ltarg = cw.util.get_linktarget(path);
                if (!os.path.isfile(ltarg)) {
                    var spath = cw.util.join_paths(ltarg, "Summary.wsm");
                    if (os.path.isfile(spath)) {
                        // クラシックなシナリオ
                        dbpaths.append(path);
                        update_path(t, spath, path);
                        continue;
                    }
                    spath = cw.util.join_paths(ltarg, "Summary.xml");
                    if (os.path.isfile(spath)) {
                        // 展開済みのシナリオ
                        dbpaths.append(path);
                        update_path(t, spath, path);
                        continue;
                    }
                    this.delete(path, false);
                } else {
                    dbpaths.append(path);
                    update_path(t, ltarg, path);
                }
            }
            if (commit) {
                this.con.commit();
            }
            dbpaths = new HashSet<object>(dbpaths);
            foreach (var path in get_scenariopaths(dpath)) {
                if (!dbpaths.Contains(path)) {
                    this._insert_scenario(path, false, skintype: skintype);
                }
            }
            if (commit) {
                this.con.commit();
            }
        }
        
        // 肥大化したDBファイルのサイズを最適化する。
        public virtual object vacuum(object commit = true) {
            // 存在しないディレクトリが含まれる場合は除去
            var s = "SELECT dpath FROM scenariodb GROUP BY dpath";
            this.cur.execute(s);
            var res = this.cur.fetchall();
            foreach (var t in res) {
                var dpath = t[0];
                if (!dpath || !os.path.isdir(dpath)) {
                    s = "DELETE FROM scenariodb WHERE dpath=?";
                    this.cur.execute(s, Tuple.Create(dpath));
                    s = "DELETE FROM scenarioimage WHERE dpath=?";
                    this.cur.execute(s, Tuple.Create(dpath));
                    s = "DELETE FROM scenariotype WHERE dpath=?";
                    this.cur.execute(s, Tuple.Create(dpath));
                }
            }
            // データ量によっては処理に秒単位で時間がかかる上、
            // 再利用可能な領域が減ってパフォーマンスが落ちるため実施しない
            //#s = "VACUUM scenariodb, scenariotype"
            //#self.cur.execute(s)
            //#s = "VACUUM scenariotype"
            //#self.cur.execute(s)
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object delete(object path, object commit = true) {
            path = path.replace("\\", "/");
            var _tup_1 = os.path.split(path);
            var dpath = _tup_1.Item1;
            var fname = _tup_1.Item2;
            var s = "DELETE FROM scenariodb WHERE dpath=? AND fname=?";
            this.cur.execute(s, Tuple.Create(dpath, fname));
            s = "DELETE FROM scenarioimage WHERE dpath=? AND fname=?";
            this.cur.execute(s, Tuple.Create(dpath, fname));
            s = "DELETE FROM scenariotype WHERE dpath=? AND fname=?";
            this.cur.execute(s, Tuple.Create(dpath, fname));
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object delete_all(object commit = true) {
            var s = "DELETE FROM scenariodb";
            this.cur.execute(s);
            s = "DELETE FROM scenarioimage";
            this.cur.execute(s);
            s = "DELETE FROM scenariotype";
            this.cur.execute(s);
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object insert(object t, object images, object commit = true, object skintype = "") {
            object postype;
            var s = @"INSERT OR REPLACE INTO scenariodb(
                    dpath, type, fname, name, author, desc, skintype,
                    levelmin, levelmax, coupons, couponsnum,
                    startid, tags, ctime, mtime, wsnversion, image, imgpath
               ) VALUES (
                    ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?
               )";
            this.cur.execute(s, t);
            if (skintype) {
                s = @"INSERT OR REPLACE INTO scenariotype
                   VALUES(?, ?, ?)";
                this.cur.execute(s, Tuple.Create(t[0], t[2], skintype));
            }
            if (images) {
                s = @"
            DELETE FROM scenarioimage WHERE dpath=? AND fname=?
            ";
                this.cur.execute(s, Tuple.Create(t[0], t[2]));
                foreach (var _tup_1 in images.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                    var i = _tup_1.Item1;
                    var image = _tup_1.Item2;
                    s = @"
                INSERT OR REPLACE INTO scenarioimage (
                    dpath,
                    fname,
                    numorder,
                    scale,
                    image,
                    imgpath,
                    postype
                ) VALUES (
                    ?,
                    ?,
                    ?,
                    ?,
                    ?,
                    ?,
                    ?
                )
                ";
                    if (image[1].postype == "Default") {
                        postype = null;
                    } else {
                        postype = image[1].postype;
                    }
                    this.cur.execute(s, Tuple.Create(t[0], t[2], i, image[2], image[0], image[1].path, postype));
                }
            }
            if (commit) {
                this.con.commit();
            }
        }
        
        // データベースにシナリオを登録する。
        [synclock(_lock)]
        public virtual object insert_scenario(object path, object commit = true, object skintype = "") {
            this._insert_scenario(path, commit, skintype: skintype);
        }
        
        public virtual object _insert_scenario(object path, object commit = true, object skintype = "") {
            var _tup_1 = read_summary(path);
            var t = _tup_1.Item1;
            var images = _tup_1.Item2;
            if (t) {
                this.insert(t, images, commit, skintype: skintype);
                return true;
            } else if (path.startswith("Scenario")) {
                // 登録できなかったファイルを移動
                // (Scenarioフォルダ内のみ)
                //#dname = "UnregisteredScenario"
                //#if not os.path.isdir(dname):
                //#    os.makedirs(dname)
                //#dst = cw.util.join_paths(dname, os.path.basename(path))
                //#dst = cw.util.dupcheck_plus(dst, False)
                //#shutil.move(path, dst)
                return false;
            }
        }
        
        // 
        //         データベース内のシナリオ情報からヘッダ部分を返す。
        //         情報が古くなっている場合は更新する。
        //         
        public virtual object create_header(object data, object skintype = "", object update = true) {
            object imgdbrec;
            if (!data) {
                return null;
            }
            if (data["image"] == null) {
                var s = @"
                SELECT
                    scale,
                    image,
                    imgpath,
                    postype
                FROM
                    scenarioimage
                WHERE
                    dpath=? AND fname=?
                ORDER BY
                    numorder
            ";
                imgdbrec = this.cur.execute(s, Tuple.Create(data["dpath"], data["fname"]));
            } else {
                imgdbrec = null;
            }
            var header = cw.header.ScenarioHeader(data, imgdbrec: imgdbrec);
            if (!update) {
                return header;
            }
            var path = header.get_fpath();
            var ltarg = cw.util.get_linktarget(path);
            if (!os.path.isfile(ltarg)) {
                var spath = cw.util.join_paths(ltarg, "Summary.wsm");
                if (os.path.isfile(spath)) {
                    return func(spath, header);
                }
                spath = cw.util.join_paths(ltarg, "Summary.xml");
                if (os.path.isfile(spath)) {
                    return func(spath, header);
                }
                this.delete(path);
                return null;
            } else if (os.path.getmtime(ltarg) > header.mtime) {
                if (this._insert_scenario(path)) {
                    // 更新後の情報を取得
                    header = this._search_path(path, skintype: skintype);
                } else {
                    return null;
                }
            }
            Func<object, object, object> func = (spath,header) => {
                // クラシックなシナリオ
                if (os.path.getmtime(spath) > header.mtime) {
                    var _tup_1 = read_summary(path);
                    var cs = _tup_1.Item1;
                    var images = _tup_1.Item2;
                    if (cs) {
                        this.insert(cs, images, true, skintype: skintype);
                        // 更新後の情報を取得
                        header = this._search_path(path, skintype: skintype);
                        return header;
                    }
                } else {
                    // 更新は不要
                    return header;
                }
            };
            return header;
        }
        
        // 
        //         データベース内のシナリオ群のヘッダを返す。
        //         その際、情報が古くなっている場合は更新する。
        //         
        public virtual object create_headers(object data, object skintype = "", object update = true) {
            var headers = new List<object>();
            var names = new HashSet<object>();
            foreach (var t in data) {
                var header = this.create_header(t, skintype: skintype, update: update);
                if (header) {
                    headers.append(header);
                    names.add(header.fname);
                }
            }
            return Tuple.Create(headers, names);
        }
        
        public virtual object sort_headers(object headers) {
            cw.util.sort_by_attr(headers, "levelmin", "levelmax", "name", "author", "fname", "mtime_reversed");
            return headers;
        }
        
        [synclock(_lock)]
        public virtual object search_path(object path, object skintype = "") {
            return this._search_path(path, skintype: skintype);
        }
        
        public virtual object _search_path(object path, object skintype = "") {
            path = path.replace("\\", "/");
            var _tup_1 = os.path.split(path);
            var dpath = _tup_1.Item1;
            var fname = _tup_1.Item2;
            this._fetch(dpath, fname, skintype);
            var data = this.cur.fetchone();
            var ltarg = cw.util.get_linktarget(path);
            if (!data && os.path.exists(ltarg)) {
                if (this._insert_scenario(path, skintype: skintype)) {
                    this._fetch(dpath, fname, skintype);
                    data = this.cur.fetchone();
                }
            }
            return this.create_header(data, skintype: skintype);
        }
        
        public object FETCH_SQL = "SELECT" + "     A.dpath," + "     A.type," + "     A.fname," + "     A.name," + "     A.author," + "     A.desc," + "     A.skintype," + "     A.levelmin," + "     A.levelmax," + "     A.coupons," + "     A.couponsnum," + "     A.startid," + "     A.tags," + "     A.ctime," + "     A.mtime," + "     A.image," + "     A.imgpath," + "     A.wsnversion";
        
        public virtual object _fetch(object dpath, object fname, object skintype) {
            object s;
            if (skintype) {
                s = Scenariodb.FETCH_SQL + " FROM scenariodb A LEFT JOIN scenariotype B" + " ON A.dpath=B.dpath AND A.fname=B.fname" + " WHERE A.dpath=? AND A.fname=? AND (B.skintype=? OR B.skintype IS NULL)";
                this.cur.execute(s, Tuple.Create(dpath, fname, skintype));
            } else {
                s = Scenariodb.FETCH_SQL + " FROM scenariodb A WHERE dpath=? AND fname=?";
                this.cur.execute(s, Tuple.Create(dpath, fname));
            }
        }
        
        public virtual object _fetch_from_name(object name, object author, object skintype) {
            object s;
            if (skintype) {
                s = Scenariodb.FETCH_SQL + " FROM scenariodb A LEFT JOIN scenariotype B" + " ON A.dpath=B.dpath AND A.fname=B.fname" + " WHERE A.name=? AND A.author=? AND (B.skintype=? OR B.skintype IS NULL)" + " ORDER BY A.mtime DESC, A.dpath, A.fname";
                this.cur.execute(s, Tuple.Create(name, author, skintype));
            } else {
                s = Scenariodb.FETCH_SQL + " FROM scenariodb A WHERE name=? AND author=?" + " ORDER BY mtime DESC, dpath, fname";
                this.cur.execute(s, Tuple.Create(name, author));
            }
        }
        
        [synclock(_lock)]
        public virtual object search_dpath(object dpath, object create = false, object skintype = "", object update = true) {
            object s;
            dpath = cw.util.get_linktarget(dpath).replace("\\", "/");
            if (skintype) {
                s = "SELECT" + "     A.dpath," + "     A.type," + "     A.fname," + "     A.name," + "     A.author," + "     A.desc," + "     A.skintype," + "     A.levelmin," + "     A.levelmax," + "     A.coupons," + "     A.couponsnum," + "     A.startid," + "     A.tags," + "     A.ctime," + "     A.mtime," + "     A.image," + "     A.imgpath," + "     A.wsnversion" + " FROM scenariodb A LEFT JOIN scenariotype B" + " ON A.dpath=B.dpath AND A.fname=B.fname" + " WHERE A.dpath=? AND (B.skintype=? OR B.skintype IS NULL)";
                this.cur.execute(s, Tuple.Create(dpath, skintype));
            } else {
                s = "SELECT" + "     A.dpath," + "     A.type," + "     A.fname," + "     A.name," + "     A.author," + "     A.desc," + "     A.skintype," + "     A.levelmin," + "     A.levelmax," + "     A.coupons," + "     A.couponsnum," + "     A.startid," + "     A.tags," + "     A.ctime," + "     A.mtime," + "     A.image," + "     A.imgpath," + "     A.wsnversion" + " FROM scenariodb A WHERE dpath=?";
                this.cur.execute(s, Tuple.Create(dpath));
            }
            var data = this.cur.fetchall();
            var _tup_1 = this.create_headers(data, skintype: skintype, update: update);
            var headers = _tup_1.Item1;
            var names = _tup_1.Item2;
            if (!update) {
                return this.sort_headers(headers);
            }
            // データベースに登録されていないシナリオファイルがないかチェック
            var dbpaths = new HashSet<object>(headers.Select(h => h.get_fpath()));
            if (!os.path.exists(dpath)) {
                if (create) {
                    os.makedirs(dpath);
                } else {
                    return new List<object>();
                }
            }
            if (!os.path.isdir(dpath)) {
                return new List<object>();
            }
            foreach (var name in os.listdir(unicode(dpath))) {
                if (names.Contains(name)) {
                    continue;
                }
                var path = cw.util.join_paths(dpath, name);
                var ltarg = cw.util.get_linktarget(path);
                var name = os.path.basename(ltarg);
                var lname = name.lower();
                if (!dbpaths.Contains(path) && os.path.isfile(ltarg) && (lname.endswith(".wsn") || lname.endswith(".zip") || lname.endswith(".lzh") || lname.endswith(".cab"))) {
                    var header = this._search_path(path, skintype: skintype);
                    if (header) {
                        headers.append(header);
                    }
                }
            }
            return this.sort_headers(headers);
        }
        
        [synclock(_lock)]
        public virtual object get_header(object path, object skintype = "") {
            var dpath = os.path.dirname(path);
            var fname = os.path.basename(path);
            this._fetch(dpath, fname, skintype);
            var data = this.cur.fetchall();
            foreach (var t in data) {
                return this.create_header(t, skintype: skintype);
            }
            return null;
        }
        
        [synclock(_lock)]
        public virtual object find_headers(object ftypes, object value, object skintype = "") {
            object s;
            object intv;
            var where = new List<object>();
            var values = new List<object>();
            Func<object, object> encode_like = value => {
                var value2 = value.replace("\\", "\\\\");
                value2 = value2.replace("%", "\\%");
                value2 = value2.replace("_", "\\_");
                value2 = "%" + value2 + "%";
                return value2;
            };
            foreach (var ftype in ftypes) {
                if (ftype == DATA_TITLE) {
                    where.append("name LIKE ? ESCAPE '\\'");
                    values.append(encode_like(value));
                } else if (ftype == DATA_DESC) {
                    where.append("desc LIKE ? ESCAPE '\\'");
                    values.append(encode_like(value));
                } else if (ftype == DATA_AUTHOR) {
                    where.append("author LIKE ? ESCAPE '\\'");
                    values.append(encode_like(value));
                } else if (ftype == DATA_LEVEL) {
                    try {
                        intv = Convert.ToInt32(value);
                        where.append("levelmin <= ? AND ? <= levelmax");
                        values.append(intv);
                        values.append(intv);
                    } catch {
                        intv = null;
                    }
                } else if (ftype == DATA_FNAME) {
                    where.append("A.fname LIKE ? ESCAPE '\\'");
                    values.append(encode_like(value));
                } else {
                    throw Exception();
                }
            }
            where = "(" + ") OR (".join(where) + ")";
            if (skintype) {
                s = "SELECT" + "     A.dpath," + "     A.type," + "     A.fname," + "     A.name," + "     A.author," + "     A.desc," + "     A.skintype," + "     A.levelmin," + "     A.levelmax," + "     A.coupons," + "     A.couponsnum," + "     A.startid," + "     A.tags," + "     A.ctime," + "     A.mtime," + "     A.image," + "     A.imgpath," + "     A.wsnversion" + " FROM scenariodb A LEFT JOIN scenariotype B" + " ON A.dpath=B.dpath AND A.fname=B.fname" + " WHERE (" + where + ")\"     AND (B.skintype=? OR B.skintype IS NULL)\"";
                values = tuple(values) + Tuple.Create(skintype);
            } else {
                s = "SELECT" + "     A.dpath," + "     A.type," + "     A.fname," + "     A.name," + "     A.author," + "     A.desc," + "     A.skintype," + "     A.levelmin," + "     A.levelmax," + "     A.coupons," + "     A.couponsnum," + "     A.startid," + "     A.tags," + "     A.ctime," + "     A.mtime," + "     A.image," + "     A.imgpath," + "     A.wsnversion" + " FROM scenariodb A WHERE (" + where + ")";
                values = tuple(values) + Tuple.Create("<Empty>");
            }
            this.cur.execute(s, values);
            var data = this.cur.fetchall();
            // 検索ではスキン情報は更新しない
            var _tup_1 = this.create_headers(data, skintype: "");
            var headers = _tup_1.Item1;
            var _names = _tup_1.Item2;
            var v = value.lower();
            // 情報が更新されている可能性があるため再チェック
            var paths = new HashSet<object>();
            var seq = new List<object>();
            foreach (var header in headers) {
                if (ftypes.Contains(DATA_TITLE) && header.name.lower().Contains(v) || ftypes.Contains(DATA_AUTHOR) && header.author.lower().Contains(v) || ftypes.Contains(DATA_DESC) && header.desc.lower().Contains(v) || ftypes.Contains(DATA_LEVEL) && !(intv == null) && header.levelmin <= intv <= header.levelmax || ftypes.Contains(DATA_FNAME) && header.fname.lower().Contains(v)) {
                    var fpath = header.get_fpath();
                    fpath = os.path.abspath(fpath);
                    fpath = os.path.normpath(fpath);
                    fpath = os.path.normcase(fpath);
                    if (!paths.Contains(fpath)) {
                        paths.add(fpath);
                        seq.append(header);
                    }
                }
            }
            return this.sort_headers(seq);
        }
        
        // 
        //         シナリオ名と作者名からシナリオDBを検索する。
        //         ただしファイルパスがignore_dpathとignore_fnameにマッチするシナリオは無視する。
        //         
        [synclock(_lock)]
        public virtual object find_scenario(
            object name,
            object author,
            object skintype,
            object ignore_dpath = null,
            object ignore_fname = null) {
            this._fetch_from_name(name, author, skintype);
            var data = this.cur.fetchall();
            ignore_dpath = os.path.normcase(os.path.normpath(os.path.abspath(ignore_dpath)));
            ignore_fname = os.path.normcase(ignore_fname);
            var seq = new List<object>();
            foreach (var t in data) {
                var dpath = os.path.normcase(os.path.normpath(os.path.abspath(t["dpath"])));
                var fname = os.path.normcase(t["fname"]);
                if (dpath == ignore_dpath && fname == ignore_fname) {
                    continue;
                }
                var header = this.create_header(t, skintype: skintype);
                if (header) {
                    seq.append(header);
                }
            }
            return seq;
        }
        
        // 
        //         ディレクトリ名の変更を通知し、サブディレクトリ内の情報を更新する。
        //         
        [synclock(_lock)]
        public virtual object rename_dir(object before, object after) {
            var orig_before = before;
            var orig_after = after;
            before = before.replace("%", "\\%");
            before += "/%";
            after = after.replace("%", "\\%");
            after += "/%";
            foreach (var tablename in Tuple.Create("scenariodb", "scenarioimage", "scenariotype")) {
                var s = String.Format("SELECT * FROM %s WHERE dpath LIKE ? ESCAPE '\\'", tablename);
                this.cur.execute(s, Tuple.Create(before));
                foreach (var d in this.cur.fetchall()) {
                    s = String.Format("UPDATE %s SET dpath=? WHERE dpath=? AND fname=?", tablename);
                    var ndpath = d["dpath"].replace(orig_before + "/", orig_after + "/", 1);
                    this.cur.execute(s, Tuple.Create(ndpath, d["dpath"], d["fname"]));
                }
                s = String.Format("UPDATE %s SET dpath=? WHERE dpath=?", tablename);
                this.cur.execute(s, Tuple.Create(orig_after, orig_before));
            }
        }
        
        // 
        //         ディレクトリの削除を通知する。
        //         
        [synclock(_lock)]
        public virtual object remove_dir(object dpath) {
            object s;
            var orig_dpath = dpath;
            dpath = dpath.replace("%", "\\%");
            dpath += "/%";
            foreach (var tablename in Tuple.Create("scenariodb", "scenarioimage", "scenariotype")) {
                s = String.Format("DELETE FROM %s WHERE dpath LIKE ? ESCAPE '\\'", tablename);
                this.cur.execute(s, Tuple.Create(dpath));
            }
            s = String.Format("DELETE FROM %s WHERE dpath=?", tablename);
            this.cur.execute(s, Tuple.Create(orig_dpath));
        }
        
        [synclock(_lock)]
        public virtual object commit() {
            this.con.commit();
        }
        
        public virtual object close() {
            this.con.close();
        }
    }
    
    // dpath以下のシナリオが存在しうる
    //     ディレクトリの一覧を取得する。
    //     シナリオのディレクトリ自体は除外される。
    //     
    public static object find_alldirectories(object dpath, object is_cancel = null) {
        var result = new HashSet<object>();
        var exclude = new HashSet<object>();
        _find_alldirectories(dpath, result, exclude, is_cancel);
        return result;
    }
    
    public static object _find_alldirectories(object dpath, object result, object exclude, object is_cancel) {
        dpath = cw.util.get_linktarget(dpath);
        var abs = os.path.abspath(dpath);
        abs = os.path.normpath(abs);
        abs = os.path.normcase(abs);
        if (exclude.Contains(abs)) {
            return;
        }
        exclude.add(abs);
        result.add(dpath);
        foreach (var fname in os.listdir(dpath)) {
            if (is_cancel && is_cancel()) {
                return;
            }
            var dpath2 = cw.util.join_paths(dpath, fname);
            dpath2 = cw.util.get_linktarget(dpath2);
            if (!os.path.isdir(dpath2) || is_scenario(dpath2)) {
                continue;
            }
            _find_alldirectories(dpath2, result, exclude, is_cancel);
        }
    }
    
    // 
    //     指定されたパスがシナリオならTrueを返す。
    //     
    public static object is_scenario(object path) {
        var ltarg = cw.util.get_linktarget(path);
        if (os.path.isdir(ltarg)) {
            var spath = cw.util.join_paths(ltarg, "Summary.wsm");
            if (os.path.isfile(spath)) {
                return true;
            }
            spath = cw.util.join_paths(ltarg, "Summary.xml");
            if (os.path.isfile(spath)) {
                return true;
            }
            return false;
        } else {
            var lpath = ltarg.lower();
            return lpath.endswith(".wsn") || lpath.endswith(".zip") || lpath.endswith(".lzh") || lpath.endswith(".cab");
        }
    }
    
    public static object read_summary(object basepath) {
        object fdata;
        object scedir;
        object dpath;
        object imgbuf;
        object scale;
        object imgpath;
        object imgbufs;
        object summaryinfos;
        object imgpaths;
        object can_loaded_scaledimage;
        object e;
        object rootattrs;
        object spath;
        object f;
        Func<object, object, object> imgbufs_to_result = (summaryinfos,imgbufs) => {
            if (imgbufs.Count == 0) {
                var imgbuf = "";
                object imgpath = null;
            } else if (imgbufs.Count == 1 && imgbufs[0][1].postype == "Default" && imgbufs[0][2] == 1) {
                imgbuf = imgbufs[0][0];
                imgpath = imgbufs[0][1].path;
                imgbufs = new List<object>();
            } else {
                imgbuf = null;
                imgpath = null;
            }
            summaryinfos.append(imgbuf);
            summaryinfos.append(imgpath);
            return Tuple.Create(tuple(summaryinfos), imgbufs);
        };
        var path = cw.util.get_linktarget(basepath);
        if (os.path.isdir(path)) {
            f = null;
            try {
                spath = cw.util.join_paths(path, "Summary.wsm");
                if (os.path.isfile(spath)) {
                    using (var f = cw.binary.cwfile.CWFile(spath, "rb", decodewrap: true)) {
                        _tup_1 = read_summary_classic(basepath, spath, f);
                        r = _tup_1.Item1;
                        images = _tup_1.Item2;
                        f.close();
                    }
                    return Tuple.Create(r, images);
                }
                spath = cw.util.join_paths(path, "Summary.xml");
                if (os.path.isfile(spath)) {
                    rootattrs = new Dictionary<object, object> {
                    };
                    e = cw.data.xml2element(spath, "Property", rootattrs: rootattrs);
                    can_loaded_scaledimage = cw.util.str2bool(rootattrs.get("scaledimage", "False"));
                    var _tup_2 = parse_summarydata(basepath, e, TYPE_WSN, os.path.getmtime(spath), rootattrs);
                    imgpaths = _tup_2.Item1;
                    summaryinfos = _tup_2.Item2;
                    imgbufs = new List<object>();
                    foreach (var info in imgpaths) {
                        imgpath = cw.util.join_paths(path, info.path);
                        foreach (var _tup_3 in cw.util.get_scaledimagepaths(imgpath, can_loaded_scaledimage)) {
                            imgpath = _tup_3.Item1;
                            scale = _tup_3.Item2;
                            if (os.path.isfile(imgpath)) {
                                using (var f2 = open(imgpath, "rb")) {
                                    imgbuf = f2.read();
                                    f2.close();
                                }
                                imgbuf = buffer(imgbuf);
                                imgbufs.append(Tuple.Create(imgbuf, info, scale));
                            } else if (scale == 1) {
                                imgbufs.append(Tuple.Create(null, info, scale));
                            }
                        }
                    }
                    return imgbufs_to_result(summaryinfos, imgbufs);
                }
            } catch {
                cw.util.print_ex();
                return Tuple.Create(null, new List<object>());
            }
        }
        if (path.lower().endswith(".cab")) {
            try {
                var summpath = cw.util.cab_hasfile(path, "Summary.wsm");
                if (summpath) {
                    // BUG: Windows XP付属のexpandのバージョン5とより新しいバージョン6では
                    //      expandの-fオプションの挙動が違う。
                    //      5ではCABアーカイブ内のパスを指定しなければ失敗し、
                    //      6ではパスを指定すると失敗しファイル名を指定すると成功する。
                    //      ワイルドカード指定はどちらでも成功する。
                    dpath = cw.util.join_paths(cw.tempdir, "Cab");
                    if (!os.path.isdir(dpath)) {
                        os.makedirs(dpath);
                    }
                    var s = String.Format("expand \"%s\" -f:\"%s\" \"%s\"", path, "*.wsm", dpath);
                    var encoding = sys.getfilesystemencoding();
                    var ret = subprocess.call(s.encode(encoding), shell: true);
                    if (ret == 0) {
                        spath = cw.util.join_paths(dpath, os.path.basename(summpath));
                        if (!os.path.isfile(spath)) {
                            spath = cw.util.join_paths(dpath, summpath);
                        }
                        if (os.path.isfile(spath)) {
                            f = null;
                            try {
                                using (var f = cw.binary.cwfile.CWFile(spath, "rb", decodewrap: true)) {
                                    _tup_4 = read_summary_classic(basepath, path, f);
                                    r = _tup_4.Item1;
                                    images = _tup_4.Item2;
                                    f.close();
                                    return Tuple.Create(r, images);
                                }
                            } finally {
                                foreach (var fpath in os.listdir(dpath)) {
                                    var fpath = cw.util.decode_zipname(fpath);
                                    fpath = cw.util.join_paths(dpath, fpath);
                                    cw.util.remove(fpath);
                                }
                            }
                        }
                    } else {
                        return Tuple.Create(null, new List<object>());
                    }
                } else {
                    summpath = cw.util.cab_hasfile(path, "Summary.xml");
                    if (summpath) {
                        scedir = os.path.dirname(summpath);
                        dpath = cw.util.join_paths(cw.tempdir, "Cab");
                        if (!os.path.isdir(dpath)) {
                            os.makedirs(dpath);
                        }
                        s = String.Format("expand \"%s\" -f:%s \"%s\"", path, "Summary.xml", dpath);
                        encoding = sys.getfilesystemencoding();
                        ret = subprocess.call(s.encode(encoding), shell: true);
                        var summpath2 = cw.util.join_paths(dpath, summpath);
                        if (ret == 0 && os.path.isfile(summpath2)) {
                            try {
                                rootattrs = new Dictionary<object, object> {
                                };
                                e = cw.data.xml2element(summpath2, "Property", rootattrs: rootattrs);
                                can_loaded_scaledimage = cw.util.str2bool(rootattrs.get("scaledimage", "False"));
                                try {
                                    var _tup_5 = parse_summarydata(basepath, e, TYPE_WSN, os.path.getmtime(path), rootattrs);
                                    imgpaths = _tup_5.Item1;
                                    summaryinfos = _tup_5.Item2;
                                } catch {
                                    return Tuple.Create(null, new List<object>());
                                }
                                imgbufs = new List<object>();
                                foreach (var info in imgpaths) {
                                    imgpath = cw.util.join_paths(scedir, info.path);
                                    foreach (var _tup_6 in cw.util.get_scaledimagepaths(imgpath, can_loaded_scaledimage)) {
                                        imgpath = _tup_6.Item1;
                                        scale = _tup_6.Item2;
                                        s = String.Format("expand \"%s\" -f:\"%s\" \"%s\"", path, os.path.basename(imgpath), dpath);
                                        encoding = sys.getfilesystemencoding();
                                        ret = subprocess.call(s.encode(encoding), shell: true);
                                        var imgpath2 = cw.util.join_paths(dpath, imgpath);
                                        if (ret == 0 && os.path.isfile(imgpath2)) {
                                            using (var f = open(imgpath2, "rb")) {
                                                imgbuf = f.read();
                                                f.close();
                                            }
                                            imgbuf = buffer(imgbuf);
                                            imgbufs.append(Tuple.Create(imgbuf, info, scale));
                                        } else if (scale == 1) {
                                            imgbufs.append(Tuple.Create(null, info, scale));
                                        }
                                    }
                                }
                                return imgbufs_to_result(summaryinfos, imgbufs);
                            } finally {
                                foreach (var p in os.listdir(dpath)) {
                                    cw.util.remove(cw.util.join_paths(dpath, p));
                                }
                            }
                        }
                    }
                    return Tuple.Create(null, new List<object>());
                }
            } catch (Exception) {
                cw.util.print_ex();
                return Tuple.Create(null, new List<object>());
            }
        }
        if (os.path.isdir(path)) {
            return Tuple.Create(null, new List<object>());
        }
        object z = null;
        try {
            z = cw.util.zip_file(path, "r");
            var names = z.namelist();
            var nametable = new Dictionary<object, object> {
            };
            var seq = new List<object>();
            foreach (var name in names) {
                nametable[cw.util.join_paths(cw.util.decode_zipname(name))] = name;
                if (name.lower().endswith("summary.xml") || name.lower().endswith("summary.wsm")) {
                    seq.append(name);
                }
            }
            if (!seq) {
                z.close();
                return Tuple.Create(null, new List<object>());
            }
            var name = seq[0];
            if (name.lower().endswith(".wsm")) {
                fdata = z.read(name);
                f = cw.binary.cwfile.CWFile("", "rb", decodewrap: true, f: io.BytesIO(fdata));
                return read_summary_classic(basepath, path, f);
            }
            scedir = os.path.dirname(name);
            scedir = cw.util.decode_zipname(scedir);
            fdata = z.read(name);
            f = StringIO.StringIO(fdata);
            try {
                rootattrs = new Dictionary<object, object> {
                };
                e = cw.data.xml2element(path, "Property", stream: f, rootattrs: rootattrs);
            } finally {
                f.close();
            }
            var _tup_7 = parse_summarydata(basepath, e, TYPE_WSN, os.path.getmtime(path), rootattrs);
            imgpaths = _tup_7.Item1;
            summaryinfos = _tup_7.Item2;
            can_loaded_scaledimage = cw.util.str2bool(rootattrs.get("scaledimage", "False"));
            imgbufs = new List<object>();
            foreach (var info in imgpaths) {
                imgpath = cw.util.join_paths(scedir, info.path);
                foreach (var _tup_8 in cw.util.get_scaledimagepaths(imgpath, can_loaded_scaledimage)) {
                    imgpath = _tup_8.Item1;
                    scale = _tup_8.Item2;
                    imgpath = nametable.get(imgpath, "");
                    if (imgpath) {
                        imgbuf = cw.util.read_zipdata(z, imgpath);
                        if (imgbuf) {
                            imgbuf = buffer(imgbuf);
                            imgbufs.append(Tuple.Create(imgbuf, info, scale));
                        } else {
                            imgbufs.append(Tuple.Create(null, info, scale));
                        }
                    } else if (scale == 1) {
                        imgbufs.append(Tuple.Create(null, info, scale));
                    }
                }
            }
            z.close();
        } catch {
            cw.util.print_ex();
            Console.WriteLine(path);
            if (z) {
                z.close();
            }
            return Tuple.Create(null, new List<object>());
        }
        return imgbufs_to_result(summaryinfos, imgbufs);
    }
    
    public static object parse_summarydata(
        object basepath,
        object data,
        object scetype,
        object mtime,
        object rootattrs) {
        var e = data.find("ImagePath");
        var wsnversion = rootattrs.get("dataVersion", "");
        var imgpaths = new List<object>();
        if (!(e == null) && e.text) {
            imgpaths.append(cw.image.ImageInfo(path: e.text, postype: e.getattr(".", "positiontype", "Default")));
        }
        e = data.find("ImagePaths");
        if (!(e == null)) {
            foreach (var e2 in e) {
                if (e2.tag == "ImagePath" && e2.text) {
                    imgpaths.append(cw.image.ImageInfo(path: e2.text, postype: e2.getattr(".", "positiontype", "Default")));
                }
            }
        }
        e = data.find("Name");
        var name = e.text || "";
        e = data.find("Author");
        var author = e.text || "";
        e = data.find("Description");
        var desc = e.text || "";
        desc = cw.util.txtwrap(desc, 4);
        e = data.find("Type");
        var skintype = e.text || "";
        e = data.find("Level");
        var levelmin = Convert.ToInt32(e.get("min", 0));
        var levelmax = Convert.ToInt32(e.get("max", 0));
        e = data.find("RequiredCoupons");
        var coupons = e.text || "";
        coupons = cw.util.decodewrap(coupons);
        var couponsnum = Convert.ToInt32(e.get("number", 0));
        e = data.find("StartAreaId");
        var startid = e.text ? Convert.ToInt32(e.text) : 0;
        e = data.find("Tags");
        var tags = e.text || "";
        tags = cw.util.decodewrap(tags);
        var ctime = time.time();
        var _tup_1 = os.path.split(basepath);
        var dpath = _tup_1.Item1;
        var fname = _tup_1.Item2;
        return Tuple.Create(imgpaths, new List<object> {
            dpath,
            scetype,
            fname,
            name,
            author,
            desc,
            skintype,
            levelmin,
            levelmax,
            coupons,
            couponsnum,
            startid,
            tags,
            ctime,
            mtime,
            wsnversion
        });
    }
    
    public static object read_summary_classic(object basepath, object spath, object f = null) {
        object imgbuf;
        try {
            if (!f) {
                f = cw.binary.cwfile.CWFile(spath, "rb", decodewrap: true);
            }
            var s = cw.binary.summary.Summary(null, f, nameonly: false, materialdir: "", image_export: false);
            if (4 < s.version) {
                return Tuple.Create(null, new List<object>());
            }
            s.skintype = "";
            imgbuf = s.image;
            var ctime = time.time();
            var mtime = os.path.getmtime(spath);
        } catch (Exception) {
            return Tuple.Create(null, new List<object>());
        }
        var summaryinfos = new List<object> {
            os.path.dirname(basepath),
            TYPE_CLASSIC,
            os.path.basename(basepath),
            s.name,
            s.author,
            s.description,
            s.skintype,
            s.level_min,
            s.level_max,
            s.required_coupons,
            s.required_coupons_num,
            s.area_id,
            s.tags,
            ctime,
            mtime,
            ""
        };
        if (imgbuf) {
            imgbuf = buffer(imgbuf);
        }
        summaryinfos.append(imgbuf);
        summaryinfos.append(null);
        return Tuple.Create(tuple(summaryinfos), new List<object>());
    }
    
    public static object get_scenariopaths(object path) {
        path = cw.util.get_linktarget(path);
        if (!os.path.isdir(path)) {
            return;
        }
        foreach (var fname in os.listdir(path)) {
            var fname = cw.util.join_paths(path, fname);
            var ltarg = cw.util.get_linktarget(fname);
            if (os.path.isdir(ltarg)) {
                var fpath = cw.util.join_paths(ltarg, "Summary.wsm");
                if (os.path.isfile(fpath)) {
                    yield return fname;
                }
                fpath = cw.util.join_paths(ltarg, "Summary.xml");
                if (os.path.isfile(fpath)) {
                    yield return fname;
                }
            } else {
                var lfile = ltarg.lower();
                if (lfile.endswith(".wsn") || lfile.endswith(".zip") || lfile.endswith(".lzh") || lfile.endswith(".cab")) {
                    yield return fname;
                }
            }
        }
    }
    
    // fpathのシナリオのデータを生成して返す。
    public static object get_scenario(object fpath) {
        object images;
        object t;
        var lfpath = fpath.lower();
        if (lfpath.endswith(".wsm") || lfpath.endswith(".xml")) {
            var _tup_1 = read_summary(os.path.dirname(fpath));
            t = _tup_1.Item1;
            images = _tup_1.Item2;
        } else {
            var _tup_2 = read_summary(fpath);
            t = _tup_2.Item1;
            images = _tup_2.Item2;
        }
        if (!t) {
            return null;
        }
        var dbrec = new Dictionary<object, object> {
        }.copy();
        dbrec["dpath"] = t[0];
        dbrec["type"] = t[1];
        dbrec["fname"] = t[2];
        dbrec["name"] = t[3];
        dbrec["author"] = t[4];
        dbrec["desc"] = t[5];
        dbrec["skintype"] = t[6];
        dbrec["levelmin"] = t[7];
        dbrec["levelmax"] = t[8];
        dbrec["coupons"] = t[9];
        dbrec["couponsnum"] = t[10];
        dbrec["startid"] = t[11];
        dbrec["tags"] = t[12];
        dbrec["ctime"] = t[13];
        dbrec["mtime"] = t[14];
        dbrec["image"] = t[15];
        dbrec["imgpath"] = t[16];
        dbrec["wsnversion"] = t[17];
        var imgdbrec = new List<object>();
        foreach (var _tup_3 in images) {
            var image = _tup_3.Item1;
            var info = _tup_3.Item2;
            var scale = _tup_3.Item3;
            imgdbrec.append(new Dictionary<object, object> {
                {
                    "scale",
                    scale},
                {
                    "image",
                    image},
                {
                    "imgpath",
                    info.path},
                {
                    "postype",
                    info.postype}});
        }
        var header = cw.header.ScenarioHeader(dbrec: dbrec, imgdbrec: imgdbrec);
        return cw.data.ScenarioData(header, cardonly: true);
    }
    
    public static object main() {
        var db = Scenariodb();
        db.update();
        db.close();
    }
    
    static scenariodb() {
        main();
    }
}
