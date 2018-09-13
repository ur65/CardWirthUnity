
using cw;

using System;

public static class features {
    
    static features() {
        @"特性の定義。性別、年代、素質、特徴に派生する。";
        @"性別の定義。";
        @"年代の定義。";
        @"素質の定義。";
        @"特徴の定義。";
        @"デバグ宿で簡易生成を行う際の能力型。";
    }
    
    public class Feature
        : object {
        
        public Feature(object data) {
            this.data = data;
            // 特性名
            this.name = this.data.gettext("Name", "");
            // 器用度修正
            this.dexbonus = this.data.getfloat("Physical", "dex", 0.0);
            // 敏捷度修正
            this.aglbonus = this.data.getfloat("Physical", "agl", 0.0);
            // 知力修正
            this.intbonus = this.data.getfloat("Physical", "int", 0.0);
            // 筋力修正
            this.strbonus = this.data.getfloat("Physical", "str", 0.0);
            // 生命力修正
            this.vitbonus = this.data.getfloat("Physical", "vit", 0.0);
            // 精神力修正
            this.minbonus = this.data.getfloat("Physical", "min", 0.0);
            // 好戦-平和
            this.aggressive = this.data.getfloat("Mental", "aggressive", 0.0);
            // 社交-内向
            this.cheerful = this.data.getfloat("Mental", "cheerful", 0.0);
            // 勇敢-臆病
            this.brave = this.data.getfloat("Mental", "brave", 0.0);
            // 慎重-大胆
            this.cautious = this.data.getfloat("Mental", "cautious", 0.0);
            // 狡猾-正直
            this.trickish = this.data.getfloat("Mental", "trickish", 0.0);
        }
        
        // dataの能力値を特性によって調整する。
        public virtual object modulate(object data, object physical = true, object mental = true) {
            if (physical) {
                data.dex += this.dexbonus;
                data.agl += this.aglbonus;
                data.@int += this.intbonus;
                data.str += this.strbonus;
                data.vit += this.vitbonus;
                data.min += this.minbonus;
            }
            if (mental) {
                data.aggressive += this.aggressive;
                data.cheerful += this.cheerful;
                data.brave += this.brave;
                data.cautious += this.cautious;
                data.trickish += this.trickish;
            }
        }
        
        // modulate()と逆の調整を行う。
        public virtual object demodulate(object data, object physical = true, object mental = true) {
            if (physical) {
                data.dex -= this.dexbonus;
                data.agl -= this.aglbonus;
                data.@int -= this.intbonus;
                data.str -= this.strbonus;
                data.vit -= this.vitbonus;
                data.min -= this.minbonus;
            }
            if (mental) {
                data.aggressive -= this.aggressive;
                data.cheerful -= this.cheerful;
                data.brave -= this.brave;
                data.cautious -= this.cautious;
                data.trickish -= this.trickish;
            }
        }
    }
    
    public class Sex
        : Feature {
        
        public Sex(object data)
            : base(data) {
            // 名前の別表現。「Male」「Female」など
            this.subname = this.data.getattr(".", "subName", this.name);
            // 父親になれる性別か
            this.father = this.data.getbool(".", "father", true);
            // 母親になれる性別か
            this.mother = this.data.getbool(".", "mother", true);
        }
    }
    
    public class Period
        : Feature {
        
        public Period(object data)
            : base(data) {
            // 名前の別表現。「Child」「Young」など
            this.subname = this.data.getattr(".", "subName", this.name);
            // 略称。「CHDTV」「YNG」など
            this.abbr = this.data.getattr(".", "abbr", this.subname);
            // 子作りした際のEP消費量。0の場合は子作り不可
            this.spendep = this.data.getint(".", "spendEP", 10);
            // 初期レベル
            this.level = this.data.getint(".", "level", 1);
            // 初期クーポン
            this.coupons = data.getfind("Coupons").Select(e => Tuple.Create(e.gettext(".", ""), e.getint(".", "value", 0)));
            // キャラクタの作成時、最初から選択されている年代か
            this.firstselect = this.data.getbool(".", "firstSelect", false);
        }
    }
    
    public class Nature
        : Feature {
        
        public Nature(object data)
            : base(data) {
            // 解説
            this.description = this.data.gettext("Description", "");
            // 特殊型か
            this.special = this.data.getbool(".", "special", false);
            // 遺伝情報
            this.genecount = this.data.getint(".", "geneCount", 0);
            this.genepattern = this.data.getattr(".", "genePattern", "0000000000");
            // 最大レベル
            this.levelmax = this.data.getint(".", "levelMax", 10);
            // 派生元
            this.basenatures = data.getfind("BaseNatures").Select(e => e.gettext(".", ""));
        }
    }
    
    public class Making
        : Feature {
        
        public Making(object data)
            : base(data) {
        }
    }
    
    public class SampleType
        : Feature {
        
        public SampleType(object data)
            : base(data) {
        }
    }
    
    //!/usr/bin/env python
    // -*- coding: utf-8 -*-
    // 
    //     能力値の切り上げ・切り捨て。
    //     
    public static object wrap_ability(object data) {
        data.dex = cw.util.numwrap(data.dex, 1, data.maxdex);
        data.agl = cw.util.numwrap(data.agl, 1, data.maxagl);
        data.@int = cw.util.numwrap(data.@int, 1, data.maxint);
        data.str = cw.util.numwrap(data.str, 1, data.maxstr);
        data.vit = cw.util.numwrap(data.vit, 1, data.maxvit);
        data.min = cw.util.numwrap(data.min, 1, data.maxmin);
        data.aggressive = Convert.ToInt32(cw.util.numwrap(data.aggressive, -4, 4));
        data.cheerful = Convert.ToInt32(cw.util.numwrap(data.cheerful, -4, 4));
        data.brave = Convert.ToInt32(cw.util.numwrap(data.brave, -4, 4));
        data.cautious = Convert.ToInt32(cw.util.numwrap(data.cautious, -4, 4));
        data.trickish = Convert.ToInt32(cw.util.numwrap(data.trickish, -4, 4));
    }
}
