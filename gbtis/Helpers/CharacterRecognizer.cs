using Microsoft.Ink;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace gbtis.Helpers {
    class CharacterRecognizer {
        private Recognizer recognizer;
        private InkCanvas canvas;

        public CharacterRecognizer(InkCanvas canvas) {
            this.canvas = canvas;
            Recognizers systemRecognizers = new Recognizers();
            recognizer = systemRecognizers.GetDefaultRecognizer();
        }

        
        public string Recognize() {
            using (MemoryStream ms = new MemoryStream()) {
                // Build an inkCollector containing the strokes
                canvas.Strokes.Save(ms);
                var myInkCollector = new InkCollector();
                var ink = new Ink();
                ink.Load(ms.ToArray());

                using (RecognizerContext context = recognizer.CreateRecognizerContext()) {
                    RecognitionStatus status;
                    context.Factoid = Factoid.WordList; // Magic smoke for name recognition
                    context.Strokes = ink.Strokes;

                    // Cannot do if there are no strokes
                    if (ink.Strokes.Count > 0) {
                        var results = context.Recognize(out status);
                        if (status == RecognitionStatus.NoError) {
                            return results.ToString();
                        }
                    }
                }
            }

            return "";
        }
    }
}
