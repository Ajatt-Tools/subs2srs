//  Copyright (C) 2009-2016 Christopher Brochtrup
//
//  This file is part of subs2srs.
//
//  subs2srs is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  subs2srs is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with subs2srs.  If not, see <http://www.gnu.org/licenses/>.
//
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace subs2srs
{
    class SubsProcessor
    {
        private DateTime workerStartTime;
        private int currentStep = 0;

        public async Task StartAsync(IProgressReporter dialogProgress,
            List<List<InfoCombined>> combinedAll = null)
        {
            try { createOutputDirStructure(); }
            catch
            {
                UtilsMsg.showErrMsg("Cannot write to output directory.");
                return;
            }
        
            Logger.Instance.info("SubsProcessor.start");
            Logger.Instance.writeSettingsToLog();
        
            WorkerVars workerVars = new WorkerVars(combinedAll,
                getMediaDir(Settings.Instance.OutputDir, Settings.Instance.DeckName),
                WorkerVars.SubsProcessingType.Normal);
            this.currentStep = 0;
            dialogProgress.StepsTotal = determineNumSteps(combinedAll);
            this.workerStartTime = DateTime.Now;
        
            try
            {
                await Task.Run(() => DoWork(workerVars, dialogProgress));
            
                TimeSpan workerTotalTime = DateTime.Now - this.workerStartTime;
                string srsFormat = getSrsFormatList();
                string endMessage = String.Format(
                    "Processing completed in {0:0.00} minutes.\n\n{1}",
                    workerTotalTime.TotalMinutes, srsFormat);
                UtilsMsg.showInfoMsg(endMessage);
            }
            catch (OperationCanceledException)
            {
                UtilsMsg.showErrMsg("Action cancelled.");
            }
            catch (Exception ex)
            {
                UtilsMsg.showErrMsg($"Error: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void DoWork(WorkerVars workerVars, IProgressReporter dialogProgress)
        {
            List<List<InfoCombined>> combinedAll = new List<List<InfoCombined>>();
            WorkerSubs subsWorker = new WorkerSubs();
            int totalLines = 0;
            bool needToGenerateCombinedAll = (workerVars.CombinedAll == null);


            if (needToGenerateCombinedAll)
            {
                DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Combine subs");
                combinedAll = subsWorker.combineAllSubs(workerVars, dialogProgress);

                if (combinedAll != null) workerVars.CombinedAll = combinedAll;
                else throw new OperationCanceledException();

                foreach (List<InfoCombined> combArray in workerVars.CombinedAll)
                    totalLines += combArray.Count;

                if (totalLines == 0)
                    throw new Exception("No lines of dialog could be parsed from the subtitle files.\nPlease check that they are valid.");

                DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Inactivate lines");
                combinedAll = subsWorker.inactivateLines(workerVars, dialogProgress);

                if (combinedAll != null) workerVars.CombinedAll = combinedAll;
                else throw new OperationCanceledException();
            }

            if ((Settings.Instance.ContextLeadingCount > 0) || (Settings.Instance.ContextTrailingCount > 0))
            {
                DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Find context lines");
                combinedAll = subsWorker.markLinesOnlyNeededForContext(workerVars, dialogProgress);

                if (combinedAll != null) workerVars.CombinedAll = combinedAll;
                else throw new OperationCanceledException();
            }

            DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Remove inactive lines");
            combinedAll = subsWorker.removeInactiveLines(workerVars, dialogProgress, true);

            if (combinedAll != null) workerVars.CombinedAll = combinedAll;
            else throw new OperationCanceledException();

            totalLines = 0;
            foreach (List<InfoCombined> combArray in workerVars.CombinedAll)
                totalLines += combArray.Count;

            if (totalLines == 0)
                throw new Exception("No lines will be processed. Please check your settings to make\nsure that you are not mistakenly pruning too many lines.");

            try
            {
                if (!needToGenerateCombinedAll)
                {
                    if (!subsWorker.copyVobsubsFromPreviewDirToMediaDir(workerVars, dialogProgress))
                        throw new OperationCanceledException();
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { Logger.Instance.info($"VobSub copy failed: {ex.Message}"); }

            DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Generate import file");
            WorkerSrs srsWorker = new WorkerSrs();

            if (!srsWorker.genSrs(workerVars, dialogProgress))
                throw new OperationCanceledException();

            List<List<InfoCombined>> combinedAllWithContext = ObjectCopier.Clone<List<List<InfoCombined>>>(workerVars.CombinedAll);

            if (Settings.Instance.AudioClips.Enabled)
            {
                DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Generate audio clips");

                if (((Settings.Instance.ContextLeadingCount > 0) && Settings.Instance.ContextLeadingIncludeAudioClips) || ((Settings.Instance.ContextTrailingCount > 0) && Settings.Instance.ContextTrailingIncludeAudioClips))
                    workerVars.CombinedAll = combinedAllWithContext;
                else
                    workerVars.CombinedAll = subsWorker.removeContextOnlyLines(combinedAllWithContext);

                WorkerAudio audioWorker = new WorkerAudio();
                if (!audioWorker.genAudioClip(workerVars, dialogProgress)) throw new OperationCanceledException();
            }

            if (Settings.Instance.Snapshots.Enabled)
            {
                DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Generate snapshots");

                if (((Settings.Instance.ContextLeadingCount > 0) && Settings.Instance.ContextLeadingIncludeSnapshots) || ((Settings.Instance.ContextTrailingCount > 0) && Settings.Instance.ContextTrailingIncludeSnapshots))
                    workerVars.CombinedAll = combinedAllWithContext;
                else
                    workerVars.CombinedAll = subsWorker.removeContextOnlyLines(combinedAllWithContext);

                WorkerSnapshot snapshotWorker = new WorkerSnapshot();
                if (!snapshotWorker.genSnapshots(workerVars, dialogProgress)) throw new OperationCanceledException();
            }

            if (Settings.Instance.VideoClips.Enabled)
            {
                DialogProgress.nextStepInvoke(dialogProgress, ++currentStep, "Generate video clips");

                if (((Settings.Instance.ContextLeadingCount > 0) && Settings.Instance.ContextLeadingIncludeVideoClips) || ((Settings.Instance.ContextTrailingCount > 0) && Settings.Instance.ContextTrailingIncludeVideoClips))
                    workerVars.CombinedAll = combinedAllWithContext;
                else
                    workerVars.CombinedAll = subsWorker.removeContextOnlyLines(combinedAllWithContext);

                WorkerVideo videoWorker = new WorkerVideo();
                if (!videoWorker.genVideoClip(workerVars, dialogProgress)) throw new OperationCanceledException();
            }
        }

        private void createOutputDirStructure()
        {
            Directory.CreateDirectory(getMediaDir(Settings.Instance.OutputDir, Settings.Instance.DeckName));
        }

        private string getMediaDir(string outDir, string deckName)
        {
            return string.Format(@"{0}{1}{2}.media", outDir, Path.DirectorySeparatorChar, deckName);
        }

        private string getSrsFormatList()
        {
            string srsFormat = "Format of the Anki import file: \n";
            int listNum = 1;

            if (ConstantSettings.SrsTagFormat != "") { srsFormat += "\n" + listNum.ToString() + ") Tag"; listNum++; }
            if (ConstantSettings.SrsSequenceMarkerFormat != "") { srsFormat += "\n" + listNum.ToString() + ") Sequence Marker"; listNum++; }
            if (Settings.Instance.AudioClips.Enabled) { srsFormat += "\n" + listNum.ToString() + ") Audio clip"; listNum++; }
            if (Settings.Instance.Snapshots.Enabled) { srsFormat += "\n" + listNum.ToString() + ") Snapshot"; listNum++; }
            if (Settings.Instance.VideoClips.Enabled) { srsFormat += "\n" + listNum.ToString() + ") Video clip"; listNum++; }
            
            srsFormat += "\n" + listNum.ToString() + ") Line from Subs1"; listNum++;
            if (Settings.Instance.Subs[1].FilePattern != "") srsFormat += "\n" + listNum.ToString() + ") Line from Subs2";

            if (Settings.Instance.ContextLeadingCount > 0) srsFormat += "\n+ " + Settings.Instance.ContextLeadingCount + " leading line" + (Settings.Instance.ContextLeadingCount == 1 ? "" : "s");
            if (Settings.Instance.ContextTrailingCount > 0) srsFormat += "\n+ " + Settings.Instance.ContextTrailingCount + " trailing line" + (Settings.Instance.ContextTrailingCount == 1 ? "" : "s");

            return srsFormat;
        }

        private int determineNumSteps(List<List<InfoCombined>> combinedAll)
        {
            int numSteps = combinedAll == null ? 4 : 2;
            if ((Settings.Instance.ContextLeadingCount > 0) || (Settings.Instance.ContextTrailingCount > 0)) numSteps++;
            if (Settings.Instance.AudioClips.Enabled) numSteps++;
            if (Settings.Instance.Snapshots.Enabled) numSteps++;
            if (Settings.Instance.VideoClips.Enabled) numSteps++;
            return numSteps;
        }
    }
}
