
using random;

using copy;

using System;

public static class dice {
    
    public class Dice
        : object {
        
        public virtual object roll(object times = 1, object sided = 6) {
            if (sided <= 1) {
                return times;
            }
            var n = 0;
            foreach (var _i in xrange(times)) {
                // BUG: random.randrange()は著しく遅い
                //n += random.randrange(1, sided + 1)
                // random.uniform(1, sided+1)は多少速いが
                // 次のコードよりは遅い
                n += Convert.ToInt32(random.random() * sided) + 1;
            }
            return n;
        }
        
        public virtual object choice(object seq) {
            if (seq) {
                return random.choice(seq);
            } else {
                return null;
            }
        }
        
        public virtual object shuffle(object seq) {
            var seq2 = copy.copy(seq);
            random.shuffle(seq2);
            return seq2;
        }
        
        public virtual object pop(object seq) {
            var item = this.choice(seq);
            if (item) {
                seq.remove(item);
            }
            return item;
        }
    }
    
    //!/usr/bin/env python
    // -*- coding: utf-8 -*-
    public static object main() {
    }
    
    static dice() {
        main();
    }
}
