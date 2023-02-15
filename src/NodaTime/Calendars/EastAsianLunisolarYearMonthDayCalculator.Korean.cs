// Copyright 2023 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
//
// This class is implemented as partial because its subclasses
// contain a significant amount of data.

using System;
using System.Collections.Generic;
using System.Text;

namespace NodaTime.Calendars
{
    internal abstract partial class EastAsianLunisolarYearMonthDayCalculator
    {
        internal sealed class Korean: EastAsianLunisolarYearMonthDayCalculator
        {
            private const int AverageDaysPer10Years = 3652; // Ideally 413811/1133 ~= 365.234775 per year

            private static readonly byte[] _yearInfo = Convert.FromBase64String(
                "AhNVsAIJRdBhHaLYAhCisAIFqVBBGbSoAg1tIMIBrWACFKtgAgpVsIEfRbgCEkVwAgdSsFEbaVACDulQAgNqoBEXrVACC6tQsgFT" +
                "YAISysACB+VgcRzSqAIQ0qACBNlQMRlaqAINVqDCAqbQAhSV0AIKStBxHqTYAhKk0AIGsmBRGrVQAg6zcAEFqtARF5XQAguVsJIB" +
                "SrgCFEmwAgiksHEcqpgCEGqgAgWtUDEZTagCDStgwgKVcAIVo3ACClFwgR5ksAIR1LACB1qQURprUAIOVtACBCrgIRiTcAILouCh" +
                "H8loAhOpUAII1KBxG9qQAg+1oAIFVtAxGirYAg0l0MICktgCFZKwAgqpUJEdtKgCEaygAgatUFEbVbACDkuwAgQlsCEYkrgCDFKw" +
                "oR9pWAITWVACCGqgcRyrUAIPq2ACBUtgMRmlcAINJXDCA1KwAhXSoAIK1VCRH1qoAhJWoAIHltBRHEroAhBK0AIEpNAhGNJoAgyy" +
                "YKIBtVACE61QAgk1oGEdldACEZWwAgZJsEEapNgCDqSwwgOqWAIVaqACCq1QkR8tqAITK1ACB5VwURxJcAIPyXACBWSwIRhqUAIL" +
                "2pCiAWtQAhRm0AIJJuBhHZNwAhGS4AIGyWBBGdSoAg3UoMIC1pACFbWAAgnWsJEfJtgCEyXQAgiSsFEbqVgCD6lQAgS0oBEYtVgC" +
                "DC1QsgFVsAIUS7ACCiWwcR1SuAIRUrACBmlQMRpqqAINaqDCAqtYAhYnUAILS2CBHqVwAhKlYAIH0mBRG+kwAg7VUAIEWqARGKtQ" +
                "AgyW0LIBSugCFEnQAgmk0HEd0mgCEKpgAgW1UEEaVqgCDjWgAgKV0BEXS5gCC0WwkR+kuAISZLACB6pQYRu1SAIPa1ACBC2gIRiV" +
                "sAIMk3DCAklwAhPJcAIJZLCBHWpQAhDaUAIFW0BBGatoAg4q4AID5hAhFslwAgrJYJEe1KgCElSgAgfWUGEcWqgCEFXQAgYm0CEZ" +
                "kugCDZKwogKpWAIVqVACCbSggR21UAIRrVACB1WwQRsluAIPRXACBFKwERipWAILaVCRH3KoAhNaoAIIq1BRHEtoAhBLYAIFpXAx" +
                "GlJwAgzSYLIB6TACFNVQAgpaoIEdm5ACEZbQAgdK4EEbpOgCDqTQAgPSUCEX2SgCC7VAoR7WqAITLaACCJXQYR1K2AIQSbACBaSw" +
                "QRmyWAINalCyAbUoAhRrQAIJq2CBHpWwAhGZcAIHSXBBG2S4Ag9UsMIDalACFdpQAgtawKEfq2gCEybgAgiS4GEcyXACEMlgAgTU" +
                "oCEY2lACDNVQsgJWqAIUVbACCiXQcR6S6AISknACBqlQURrUqAIOtKACA7VQERdWqAILTbCSASWwAhOlcAIIUrBhHKlYAhBpUAIF" +
                "aqAxGK1QAgyrULICS2gCFUrgAgmlcHEeUnACEdCQAQh0mFEaaqgCDlqgAgObUCEYS2gCC0rgoR+k6AITpNACCNJgYRvVKAIPtSAC" +
                "BNagIRlW0AIMldDCAknYAhVJsAIKpLCBHapYAhFqUAIGtUBBGrWgAg2rYAIDlbAhGEm4AgxJcJEfZLgCE1SwAghqUHEcbSgCD1rA" +
                "AgSrYDEZk3ACDZLgwgHJcAIUyWACCdSggR3aUAIQtVACBlagURqq2AIPJdACA5LgIRfJWAILqVCRH9SoAhKyoAIHtVBxHFaoAhBN" +
                "oAIEpbBBGVK4Ag1SsMICqTgCFGkwAglqoIEdrVACEa5QAgZLYEEapXACDqVwAgRScCEXaTACCuUwoR9smAITWqACB1rQYRxLaAIQ" +
                "SuACBaTgQRjSaAIM0mCyAdUoAhS1IAIItqCRHVbQAhFVcAIHSdBRGqTYAg6ksAIDqlARF7UoAgq1ILEetcACEqtgAgiVsGEcSbgC" +
                "EElwAgVksDEZalgCDGpQsgFrKAIUWsACCatggR0q6AIRSeACBqTQURrSWAINslACArUgIRbykAIKtaChHpXQAhKVsAIISbBhHKS4" +
                "Ag+ksAIEqlBBGLUoAgxtQMEfrWACE6tgAgmTcIEeCXgCEklwAgdksFEbalACDtpQAgNaoBEXq2ACC6bgsgGS6AITkuACCMlgcRzU" +
                "qAIQ1KACBNVQMRlaqAINVqACAqbQERaS6AIKkrCBHqVYAhKpUAIGsqBRGrVQAg6tUAIETaARF6XQAgulcJIBUrgCFFJwAghpMHEc" +
                "apgCEGqgAgWrUDEZS6gCDUtgwgKmcAIVouACCdFggR3pMAIR1KACBtqgURpbUAIOVtACBErgIRii6AILotChH9FYAhOqUAIItSBx" +
                "G9agAg+toAIFVdAxGknYAg1FsAICorARFtFYAgqqUJEdtSgCEWsgAgatYFEbVbACDpNwAgRFcDEYorgCDFKwoR+qUAIS2VACCFqg" +
                "cRyrYAIPpuACBVLgMRnFcAINpWCyAdKoAhTSoAIJ1VCRHlqoAhFWoAIGptBRG1LoAg9SsAIDqNAhF9KoAguyoKEftVACEq1QAghN" +
                "oGEcpdACEKVwAgVRsEEZqLgCDWUwwgJqmAIUWqACCatQkR4rqAISK2ACBsNwURtRcAIOyWACA+SwMRhqkAIL2qCyAVtQAhRW0AIJ" +
                "KuBxHaLoAhGi0AIG0VBBGdSoAg21IMICtpACFa2gAgpV0JEfJdgCE0WwAgiisFEbqVgCD6lQAgS1IBEYtVACC6tgwgFVsAIUS3AC" +
                "CkVwcR1SuAIRUrACBmlQQRpsqAINWqDCAptQAhWm4AILSuCBHqVwAhKlYAIH0qBhG+lQAg7VUAIEVqAhGKrQAgyV4LIBSugCFEmw" +
                "Agmk0HEd0nACELKgAgW1UEEaVqgCDi2gAgKVsBEXSrgCC0mwkR+kuAISZLACB2qQYRutUAIPa1ACBCtgIRiVcAIMk3CyAklwAhPJ" +
                "YAII5LBxHWqQAhDaoAIFWtAxGitoAg4m4AIDkuAhFsloAgrJUJEe1KgCErSgAga2kGEbVtACD1WwAgUl0CEYktgCDJKwogGpWAIU" +
                "aVACCHSggRy1UAIQq2ACBlOwQRoluAIOJXACA1KwERepWAIKaVCRHmqoAhJaoAIHq1BRG0toAg9K4AIEpXAxGVJwAgvSoLEf2VAC" +
                "E7VQAglWoHEdltACEZXQAgdK4EEbpNgCDqTQAgPSUBEX1VACC7VQkR9WqAITLaACCJWwUR1JuAIQSXACBaSwQRmyWAINapDCAa1Q" +
                "AhRbUAIKK2CBHpVwAhGS8AIHSXBBG2SwAg7UoMIC6lACFdaQAgtW0KIBK2gCEybgAghS4GEcyWgCEMlQAgTUoCEY2lACDLWQwgJW" +
                "0AIUVbACCiXQcR6S2AISkrACBqlQURq0qAIObKACA61QERdVsAILS7CSASW4AhQlcAIIUrBhHKlQAg/pUAIFaqAxGK1QAgyrULIC" +
                "S2gCFUrgAgmlcHEeUmgCEdJgAgbZUFEaWqgCDlagAgOW0CEYSugCC0rgoR+k2AITpNACCNJQYRvVSAIPtVACBTWgIRmV0AIMlbDC" +
                "Akm4AhVJcAIKpLCBHbJYAhFqUAIGbUBBGq2oAg4rYAIDk3AhGEl4AgxJcJEfZLACEtSgAgfqUGEca0gCD1bQAgUrYDEZkvACDZLg" +
                "sgHJaAIUqVACCdSggR3aUAIQtNACBlawQRsm2AIPJdACA5LQIRfJmAILqVCRH7SoAhJqoAIHrVBhHFWoAhBLsAIFJbAxGVK4Ag1S" +
                "sLICqVACE+lQAglqoIEdrVACEZtQAgZLYEEapXACDqTwAgRSYCEW6TACCtVQoR9aqAITVqACB5bQYRxK6AIQSeACBaTQQRjSaAIM" +
                "0lCyAdUoAhS1QAIItaCBHZXQAhGVsAIHSbBBGqS4Ag6ksAIDqlARF7UoAgptQLEeragCEytgAgiTcGEcSXgCEElwAgVksDEZalAC" +
                "C9pQsgFrSAIUVtACCiugcR2S8AIRkuACBslgURrUqAIN1KACAtpQMRdaqAILVqBxHqbQAhKl0AIIktBhHKlYAg+pUAIEtKBBGLVQ" +
                "AgytUAIBVaAhFaXQAgmlsHEeUrgCEVKwAgZpMFEadKgCDmqgAgKtkDEXTagCC0tggR+lcAISpOACB9JgYRvpMAIP1SACA9qgQRhr" +
                "UAIMVtACAkrgMRWk6AIJpNBxHdJYAhGyUAIF1SBRGdagAg21oAIDVdAxF0rYAgtJ0HEfpLgCE6SwAgiqUGEctSgCEG0gAgWtoEEZ" +
                "VbACDZNwAgNJcDEXZLgCCqTQcR5qUAIR2lACB1qgURqrYAIOquACBJLgMRjJcAILyWCBH9SoAhPUoAII1VBhHFqoAhBWoAIFptBB" +
                "GlLoAg1S0AICqVAxFuSoAgq0oHEdtVACEZ1QAgdVoFEbpdACDqWwAgRSsEEYqLgCDGkwkR9qmAITaqACCKtQYR1NqAIQS2ACBaVw" +
                "QRpRcAIN0WACAekwMRZqkAIJ2qBxHltQAhFW0AIHSuBRG6TYAg+i0AID0VBBF9UoAgu1IJEf1pACEq2gAghV0GEdKtgCEUWwAgWi" +
                "sFEZsVgCDalQAgK1ICEVtZACCa1gcR5VsAISU3ACB0VwURtiuAIPUrACBGlQMRdsqAILWqChH6tQAhOm0AIISuBhHJVwAhClYAIF" +
                "0qBRGOlQAgzVUAICWqAxFqtQAgmm0HEeSugCEkqwAgeo0FEa0qgCDrKgAgO1UEEYVqgCC02gAR+V0CEVSrgCCUmwYRyouAIQZLAC" +
                "BWqQQRm1UAINa1ACAyugIReVsAILk3BhH1FwAhLRYAIH5LBRHGqQAg7akAIEW1AxGStoAg0q4AIBouAhFdFoAgnJUGEd1KgCELUg" +
                "AgW2kEEaVtACDlXQAgMl0DEXotgCC6KwcR+pWAISqVACB7SgURu1UAIPrVACBFWwQRkluAINJXCSAlK4AhRSsAIJaVBhHWyoAhFa" +
                "oAIFq1BBGlNoAg5K4AIDpXAxF1KwAgrSoHEe6VACEtVQAgdaoFEbqtACD5XQAgVK4EEYpVgCDKTQggHSWAIUspACCLVQcR1WqAIR" +
                "LaACBpXQURpKuAIOSbACA6SwMReyWAIKapCBHq1IAhJrUAIIK2BRG5WwAg+TcAIFSXBBGWSwAgvkoKEf6lACE9qQAgla0GEdK2gC" +
                "ESrgAgaS4FEayWgCDclQAgLUoDEW2lACCraQcR5W0AISVbACCCXQURyS2AIPkrACBKlQQRjUqAIMtKABH7VQIRVWqAIJVbBhHiW4" +
                "AhElcAIGUrBRGqlQAg3pUAICaqAxFq1QAgqrUIEfS2gCE0rgAgilcFEdUmgCENJgAgTZUEEZaqgCDVagAgKa0CEWSugCCkrgYR6k" +
                "2AISpNACBtJQURrZSAIOtVACBFagIReW0AILldByAUrYAhRJsAIIpLBRHLJYAhBqkAIFrUBBGLWoAg0rYAIClbAhF0m4AgpJcGEe" +
                "ZLACEeSgAgbqUFEabUgCDltQAgQrYDEYlXACC5LgcR/JaAITyVACCNSgYRvaUAIPtpACBVbQQRoq2AINJdACApLQIRbJWAIKqVBx" +
                "HdSoAhG0oAIGtVBRG1aoAg5NsAIEJbAxGJK4AgxSsIEfqVgCE2lQAghqoGEcrVACD6tQAgVLYEEZpXACDaVwAgJScDEWaTACCdlQ" +
                "cR5qqAIRVqACBprQURtK6AIPSuACA6TgQRfSaAIL0lCBH9VIAhK1QAIH1qBhHJbQAhCVsAIFSbBBGaTYAg2ksKICslgCFGpQAglt" +
                "QGEdtagCEitgAgaVsFEbSbgCD0lwAgRksDEXalACCupQgR9tSAITWtACCCtgURyTcAIQkuACBclgQRjkqAIM1KACAdpQIRZaqAIJ" +
                "VsBxHarYAhIl0AIHktBRGslYAg6pUAIDtKAxF7pQAgq1UJEfVagCE0ugAgilsFEcUrgCEFKwAgWpUEEZtKgCDGqgAgGtUCEWVagC" +
                "CktgYR2lcAIRpXACB1JwURtpMAIN2TACA1qgMRerUAILltCxH0roAhNK4AIIpNBhHNJoAg/SUAIE1SBRGNqgAgy2oAIBltAhFkrY" +
                "AgpJsHEepLgCEaSwAgayUFEatSgCDm1AAgKtoDEXlbA="
                );
            protected override byte[] YearInfo => _yearInfo;

            public Korean()
                : base(918, 2050, AverageDaysPer10Years, 0)
                // daysAtStartOfYear1 == 0 because 1 CE is out of range
            {
            }

        }
    }
}
