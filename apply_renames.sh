#!/bin/bash
FILES=(
  "Client/Pages/Admin/SakForm.razor"
  "Client/Pages/Admin/VoteringForm.razor"
  "Client/Pages/Admin/DelegatForm.razor"
  "Client/Pages/Admin/ArrangementDashboard.razor"
  "Client/Pages/Admin/ArrangementForm.razor"
  "Client/Pages/Admin/ArrangementSakDashboard.razor"
  "Client/Pages/Admin/ArrangementDelegatDashboard.razor"
  "Client/Components/AktivVotering.razor"
  "Client/Components/Delegatinfo.razor"
  "Client/Components/DelegatinfoOrLogin.razor"
  "Client/Components/DelegatFormInline.razor"
  "Client/Components/Delegatliste.razor"
  "Client/Components/EndreDelegatDialog.razor"
  "Client/Components/Enkelvotering.razor"
  "Client/Components/Flervalgsvotering.razor"
  "Client/Components/Resultatliste.razor"
  "Client/Components/ResultatlisteElement.razor"
  "Client/Components/SakFormInline.razor"
  "Client/Components/SakImport.razor"
  "Client/Components/SakImportForm.razor"
  "Client/Components/Saksliste.razor"
  "Client/Components/SakslisteLinje.razor"
  "Client/Components/ValgInline.razor"
  "Client/Components/VoteringFormInline.razor"
  "Client/Components/DelegatImport.razor"
  "Client/Components/MinSpeidingImport.razor"
  "Client/Components/MinSpeidingImport.razor.cs"
  "Client/GrpcClientServiceExtensions.cs"
  "Client/IDelegatkodeAuthService.cs"
  "Client/Program.cs"
)

for f in "${FILES[@]}"; do
  sed -i \
    -e 's/IAdminDelegatService/IAdminDelegateService/g' \
    -e 's/IDelegatNotifierService/IDelegateNotifierService/g' \
    -e 's/IDelegatService/IDelegateService/g' \
    -e 's/IAdminStemmeService/IAdminVoteService/g' \
    -e 's/IStemmeService/IVoteService/g' \
    -e 's/ISakService/ICaseService/g' \
    -e 's/AdminDelegatDto/AdminDelegateDto/g' \
    -e 's/DelegatInputModel/DelegateInputModel/g' \
    -e 's/DelegatDto/DelegateDto/g' \
    -e 's/NyDelegatDto/NewDelegateDto/g' \
    -e 's/AdminVoteringDto/AdminBallotDto/g' \
    -e 's/VoteringResultatDto/BallotResultDto/g' \
    -e 's/VoteringInputModel/BallotInputModel/g' \
    -e 's/VoteringDto/BallotDto/g' \
    -e 's/SakResultatDto/CaseResultDto/g' \
    -e 's/AdminSakDto/AdminCaseDto/g' \
    -e 's/SakInfoDto/CaseInfoDto/g' \
    -e 's/SakInputModel/CaseInputModel/g' \
    -e 's/SakDto/CaseDto/g' \
    -e 's/StemmeDto/VoteDto/g' \
    -e 's/ValgDto/ChoiceDto/g' \
    -e 's/SetTilstedeForAllRequest/SetPresentForAllRequest/g' \
    -e 's/SetTilstedeRequest/SetPresentRequest/g' \
    -e 's/TilstedeCountChangedEvent/PresentCountChangedEvent/g' \
    -e 's/TilstedeCountResponse/PresentCountResponse/g' \
    -e 's/TilstedeCountRequest/PresentCountRequest/g' \
    -e 's/HentArrangementRequest/GetArrangementRequest/g' \
    -e 's/HentVoteringerRequest/GetBallotsRequest/g' \
    -e 's/HentVoteringRequest/GetBallotRequest/g' \
    -e 's/HentDelegatRequest/GetDelegateRequest/g' \
    -e 's/SlettDelegatRequest/DeleteDelegateRequest/g' \
    -e 's/SakerRequest/CasesRequest/g' \
    -e 's/SakRequest/CaseRequest/g' \
    -e 's/AvgiStemmeRequest/CastVoteRequest/g' \
    -e 's/HarStemmtResult/HasVotedResult/g' \
    -e 's/AdminStemmeRequest/AdminBallotRequest/g' \
    -e 's/FinnAktiveVotringerRequest/GetActiveBallotsRequest/g' \
    -e 's/StemmeRequest/VoteRequest/g' \
    -e 's/LagreResult/SaveResult/g' \
    -e 's/HentResult/GetResult/g' \
    -e 's/VoteringPublisertEvent/BallotPublishedEvent/g' \
    -e 's/VoteringStoppetEvent/BallotStoppedEvent/g' \
    -e 's/VoteringStartetEvent/BallotStartedEvent/g' \
    -e 's/VoteringLukketEvent/BallotLockedEvent/g' \
    -e 's/StemmeFjernetEvent/VoteRemovedEvent/g' \
    -e 's/HarStemtEvent/VotedEvent/g' \
    -e 's/NyStemmeEvent/NewVoteEvent/g' \
    -e 's/NyVoteringEvent/NewBallotEvent/g' \
    -e 's/\.HentDelegatInfo()/.GetDelegateInfo()/g' \
    -e 's/\.HentArrangementAsync(/.GetArrangementAsync(/g' \
    -e 's/\.HentArrangementerAsync(/.GetArrangementsAsync(/g' \
    -e 's/\.HentArrangementInfoAsync(/.GetArrangementInfoAsync(/g' \
    -e 's/\.FinnAktiveVoteringer(/.GetActiveBallots(/g' \
    -e 's/\.NyttArrangement(/.CreateArrangement(/g' \
    -e 's/\.OppdaterArrangement(/.UpdateArrangement(/g' \
    -e 's/\.GetTilstedeCount(/.GetPresentCount(/g' \
    -e 's/\.HentSakInfo(/.GetCaseInfo(/g' \
    -e 's/\.HentSaker(/.GetCases(/g' \
    -e 's/\.HentSak(/.GetCase(/g' \
    -e 's/\.LagreNySak(/.CreateCase(/g' \
    -e 's/\.OppdaterSak(/.UpdateCase(/g' \
    -e 's/\.HentVoteringer(/.GetBallots(/g' \
    -e 's/\.HentVotering(/.GetBallot(/g' \
    -e 's/\.LagreNyVotering(/.CreateBallot(/g' \
    -e 's/\.OppdaterVotering(/.UpdateBallot(/g' \
    -e 's/\.HentDelegater(/.GetDelegates(/g' \
    -e 's/\.HentDelegat(/.GetDelegate(/g' \
    -e 's/\.OppdaterDelegat(/.UpdateDelegate(/g' \
    -e 's/\.RegistrerNyDelegat(/.RegisterNewDelegate(/g' \
    -e 's/\.SlettDelegat(/.DeleteDelegate(/g' \
    -e 's/\.SetTilStedeForAll(/.SetPresentForAll(/g' \
    -e 's/\.SetTilStede(/.SetPresent(/g' \
    -e 's/\.AvgiStemmeAsync(/.CastVoteAsync(/g' \
    -e 's/\.HarStemmt(/.HasVoted(/g' \
    -e 's/\.StartVotering(/.StartBallot(/g' \
    -e 's/\.StoppVotering(/.StopBallot(/g' \
    -e 's/\.LukkVotering(/.LockBallot(/g' \
    -e 's/\.PubliserVotering(/.PublishBallot(/g' \
    -e 's/\.KopierVotering(/.CopyBallot(/g' \
    -e 's/\.OnVoteringStartet(/.OnBallotStarted(/g' \
    -e 's/\.OnVoteringStoppet(/.OnBallotStopped(/g' \
    -e 's/\.OnNyStemme(/.OnNewVote(/g' \
    -e 's/\.OnStemmeFjernet(/.OnVoteRemoved(/g' \
    -e 's/\.OnVoteringLukket(/.OnBallotLocked(/g' \
    -e 's/\.OnVoteringPublisert(/.OnBallotPublished(/g' \
    -e 's/\.OnNyVotering(/.OnNewBallot(/g' \
    -e 's/\.OnHarStemt(/.OnVoted(/g' \
    -e 's/\.OnTilstedeCountChanged(/.OnPresentCountChanged(/g' \
    -e 's/\.KobleTilArrangement(/.JoinArrangement(/g' \
    -e 's/\.KobleFraArrangement(/.LeaveArrangement(/g' \
    -e 's/\.DelegaterTilstede/.DelegatesPresent/g' \
    -e 's/\.DelegaterCount/.DelegatesCount/g' \
    -e 's/\.Delegatkode/.DelegateCode/g' \
    -e 's/\.Delegatnummer/.DelegateNumber/g' \
    -e 's/\.Delegater/.Delegates/g' \
    -e 's/\.AvgitteStemmer/.CastVoteCount/g' \
    -e 's/\.AvgitStemme/.VotedDelegates/g' \
    -e 's/\.StemmeHash/.VoteHash/g' \
    -e 's/\.StemmeId/.VoteId/g' \
    -e 's/\.Stemmer/.Votes/g' \
    -e 's/\.VoteringerCount/.BallotsCount/g' \
    -e 's/\.SakerCount/.CasesCount/g' \
    -e 's/\.SakNummer/.CaseNumber/g' \
    -e 's/\.SakNavn/.CaseName/g' \
    -e 's/\.SakId/.CaseId/g' \
    -e 's/\.VoteringId/.BallotId/g' \
    -e 's/\.Voteringer/.Ballots/g' \
    -e 's/\.Saker/.Cases/g' \
    -e 's/\.ValgId/.ChoiceId/g' \
    -e 's/\.Valg/.Choices/g' \
    -e 's/\.SendtSms/.SmsSentAt/g' \
    -e 's/\.SendtEmail/.EmailSentAt/g' \
    -e 's/\.KanVelge/.MaxChoices/g' \
    -e 's/\.Tittel/.Title/g' \
    -e 's/\.Beskrivelse/.Description/g' \
    -e 's/\.Nummer/.Number/g' \
    -e 's/\.Aktiv/.Active/g' \
    -e 's/\.Hemmelig/.Secret/g' \
    -e 's/\.Lukket/.Locked/g' \
    -e 's/\.Publisert/.Published/g' \
    -e 's/\.TilStede/.Present/g' \
    -e 's/\.Startdato/.StartDate/g' \
    -e 's/\.Sluttdato/.EndDate/g' \
    -e 's/\.StartTid/.StartTime/g' \
    -e 's/\.SluttTid/.EndTime/g' \
    -e 's/\.Epost/.Email/g' \
    -e 's/\.Telefon/.Phone/g' \
    -e 's/\.Navn/.Name/g' \
    -e 's/\.DelegatId/.DelegateId/g' \
    -e 's/\.Delegat\./.Delegate./g' \
    -e 's/\.Votering\./.Ballot./g' \
    -e 's/result\.Delegat$/result.Delegate/g' \
    -e 's/result\.Delegat;/result.Delegate;/g' \
    "$f"
  echo "Done: $f"
done
