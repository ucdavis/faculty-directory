import React, { useEffect, useMemo, useState } from 'react';

import { useParams } from 'react-router-dom';
import MicRecorder from 'mic-recorder-to-mp3';

import { IBio } from '../models/IBio';
import { ISitePerson } from '../models/ISitePerson';
import { Loading } from './Loading';

const Mp3Recorder = new MicRecorder({ bitRate: 128 });

interface AudioFile {
  buffer: BlobPart[];
  type: string;
  lastModified: number;
}

export const Pronunciation = () => {
  const { id } = useParams<{ id: string }>();

  const [bio, setBio] = useState<IBio>();
  const [sitePerson, setSitePerson] = useState<ISitePerson>({} as ISitePerson);

  const [isRecording, setIsRecording] = useState(false);
  const [audioFile, setAudioFile] = useState<AudioFile>();

  // sync is enabled if audio is recorded, replaced, or deleted
  const [syncEnabled, setSyncEnabled] = useState(false);
  const [statusMessage, setStatusMessage] = useState('');

  useEffect(() => {
    const fetchPerson = async () => {
      const result = await fetch('api/sitepeople/' + id).then(r => r.json());

      if (result.sitePerson.pronunciationUid) {
        console.log('Fetching pronunciation');

        const audioFile = await fetch(`/api/faculty/${id}/pronunciation`);

        console.log('Got pronunciation', audioFile);

        const blob = await audioFile.blob();

        setAudioFile({
          buffer: [blob],
          type: audioFile.headers.get('content-type') || 'audio/mp3',
          lastModified: Date.now()
        });
      }

      setBio(result.bio);
      setSitePerson(result.sitePerson || {});
    };

    fetchPerson();
  }, [id]);

  const audioUrl = useMemo(() => {
    if (audioFile) {
      return URL.createObjectURL(
        new File(audioFile.buffer, 'me.mp3', {
          type: audioFile.type,
          lastModified: Date.now()
        })
      );
    } else {
      return '';
    }
  }, [audioFile?.lastModified]);

  const handleRecord = async () => {
    setIsRecording(true);
    setAudioFile(undefined);

    // TODO: handle error, probably because of permissions
    Mp3Recorder.start();
  };

  const handleStop = async () => {
    setIsRecording(false);

    Mp3Recorder.stop()
      .getMp3()
      .then(([buffer, blob]: any) => {
        setAudioFile({ buffer, type: blob.type, lastModified: Date.now() });
        // new audio recorded, enable sync
        setSyncEnabled(true);
      })
      .catch(console.error);
  };

  const removeRecording = () => {
    setAudioFile(undefined);

    if (sitePerson.pronunciationUid) {
      // if we removed audio but there is a pronunciation record, enable sync
      setSyncEnabled(true);
    } else {
      setSyncEnabled(false);
    }
  };

  const handleSave = async () => {
    // don't allow re-click while saving
    setSyncEnabled(false);

    if (!audioFile) {
      // no audio file, if the user has a pronunciation record, delete it
      if (sitePerson.pronunciationUid) {
        const response = await fetch(`/api/faculty/${id}/pronunciation`, {
          method: 'DELETE'
        });

        if (response.ok) {
          setStatusMessage('Pronunciation removed');
        } else {
          setStatusMessage('Error removing pronunciation');
        }
      }

      // should not be enabled, but return to be safe
      return;
    }

    // filename is the bio names + year and month of the recording
    var fileName = `${bio?.lastName}-${
      bio?.firstName
    }-${new Date().toISOString().slice(0, 7)}.mp3`;

    const formData = new FormData();
    formData.append(
      'audioFile',
      new Blob(audioFile.buffer, { type: audioFile.type }),
      fileName
    );

    const response = await fetch('api/faculty/' + id + '/pronunciation', {
      method: 'POST',
      body: formData
    });

    if (response.ok) {
      setStatusMessage('Pronunciation saved');
    } else {
      setStatusMessage('Error saving pronunciation');
    }
  };

  if (!bio || !sitePerson.person) {
    return <Loading text='LOADING...'></Loading>;
  }

  return (
    <div>
      <div className='row mt-5 justify-content-center false-recording'>
        <div className='col-md-5 card'>
          {statusMessage && (
            <div className='alert alert-primary' role='alert'>
              {statusMessage}
            </div>
          )}

          <h3>
            Pronunciation for {bio.firstName} {bio.lastName}
          </h3>
          <p>
            Use the record button below to record a pronunciation, remember to
            speak clearly into your device microphone.
          </p>
          <br />
          <div className='row d-flex justify-content-between'>
            {isRecording === false ? (
              <button className='main-btn' onClick={handleRecord}>
                Record
              </button>
            ) : (
              <button
                className='main-btn recording-button'
                onClick={handleStop}
              >
                Stop Recording
              </button>
            )}

            <button
              className='btn btn-primary'
              disabled={syncEnabled === false}
              onClick={handleSave}
            >
              Save and Sync
            </button>
          </div>

          {audioFile && (
            <div className='mt-5'>
              <>
                <audio controls src={audioUrl}>
                  Your browser does not support the
                  <code>audio</code> element.
                </audio>{' '}
                <br />
                <button className='inverse-btn' onClick={removeRecording}>
                  Remove Recording
                </button>
              </>
            </div>
          )}
        </div>
      </div>
      <div
        className={'text-center mt-5' + (isRecording ? ' recording-sheep' : '')}
      >
        <svg
          width='104px'
          height='104px'
          viewBox='0 0 104 104'
          version='1.1'
          xmlns='http://www.w3.org/2000/svg'
          className='sheep'
        >
          <title>fleece-sheep-svg</title>
          <g
            id='Current'
            stroke='none'
            strokeWidth='1'
            fill='none'
            fillRule='evenodd'
          >
            <g
              id='pronunciation'
              transform='translate(-228.000000, -665.000000)'
            >
              <g
                id='fleece-sheep-svg'
                transform='translate(228.000000, 665.000000)'
              >
                <circle
                  id='pulse-fade'
                  fill='#B4E2C8'
                  cx='52'
                  cy='52'
                  r='52'
                ></circle>
                <circle
                  id='pulse-origin'
                  fill='#71CC98'
                  cx='52'
                  cy='52'
                  r='38'
                ></circle>
                <path
                  d='M43.7241363,25.8778576 C39.076839,28.2515357 41.0988013,32.4468731 39.4537686,31.9010479 C35.2181414,30.4956581 31.372296,31.9010479 30.9209056,37.7351688 C30.9209056,37.7351688 19,37.7351688 19,41.8174483 C19,44.538968 22.1566278,46.2121271 28.4698835,46.8369258 C27.0289872,49.1147899 27.6789421,51.6434555 30.4197483,54.4229226 C30.5029923,54.5209246 23.9274011,62.1643737 33.0684359,66.7004855 C33.1370426,66.794695 32.2009733,75.3343642 41.8185378,74.5879922 C41.8158892,74.6384542 42.4709113,77.5835307 45.7568299,78.8084433 C47.2172381,79.3528489 49.2919936,78.7368509 51.9810964,76.9604493 C54.695404,78.7368509 56.7827619,79.3528489 58.2431701,78.8084433 C61.5290887,77.5835307 62.1841108,74.6384542 62.1814622,74.5879922 C71.7990267,75.3343642 70.8629574,66.794695 70.9315641,66.7004855 C80.0725989,62.1643737 73.4970077,54.5209246 73.5802517,54.4229226 C76.3210579,51.6434555 76.9710128,49.1147899 75.5301165,46.8369258 C81.8433722,46.2121271 85,44.538968 85,41.8174483 C85,37.7351688 73.0790944,37.7351688 73.0790944,37.7351688 C72.627704,31.9010479 68.7818586,30.4956581 64.5462314,31.9010479 C62.9011987,32.4468731 64.923161,28.2515357 60.2758637,25.8778576 C55.6285665,23.5041796 52.1761881,26.6961548 51.9823333,26.6961548 C51.8002563,26.6961548 48.3714335,23.5041796 43.7241363,25.8778576 Z'
                  id='fur'
                  stroke='#1F1F1F'
                  strokeWidth='2.0808'
                  fill='#F8F9FA'
                  strokeLinejoin='round'
                ></path>
                <path
                  d='M47.9804108,40.8669715 C47.9461834,40.8744346 47.9046669,40.8731367 47.8748764,40.8916322 C46.4503207,41.7644897 44.9272028,41.9403592 43.3223195,41.5879713 C43.2240743,41.5665555 43.0564236,41.6372926 42.9943072,41.7216581 C42.3756793,42.5620673 41.6090496,43.2107076 40.6883968,43.6779622 C40.5850808,43.7305283 40.4776449,43.8690823 40.4564113,43.9842735 C40.0073355,46.4373601 40.0162093,48.8784408 40.6532186,51.3036219 C41.2277947,53.4903085 42.2650741,55.4641347 43.312495,57.434067 C44.0271498,58.7777485 44.6394394,60.1402499 44.6159873,61.7337827 C44.5982397,62.9541608 44.8888555,64.1333297 45.346805,65.2654486 C46.6119499,68.3918361 49.4968734,70.1210026 52.7849192,69.7238362 C55.8593608,69.3526283 57.8011301,67.5608368 58.7674194,64.6288142 C59.0535983,63.760175 59.2627655,62.8182027 59.2643501,61.9093276 C59.2672024,60.3339658 59.7793453,58.9636768 60.4857601,57.6229158 C61.3877146,55.9112713 62.3026629,54.2080635 62.9292139,52.3578651 C63.8536698,49.6289685 63.9496965,46.8572403 63.4340676,44.0443027 C63.4068124,43.8963388 63.2733891,43.7201449 63.1421842,43.6494077 C62.2814293,43.1841 61.5379348,42.5799139 60.9624079,41.7758466 C60.8175754,41.5736941 60.6771798,41.5746675 60.4575542,41.5892693 C59.6712755,41.6411864 58.8678831,41.7550797 58.0984012,41.6454047 C57.3447652,41.5376766 56.6250397,41.1856132 55.86665,40.9331659 C53.9850955,43.7610296 49.9646474,43.7798496 47.9804108,40.8669715 M37.7198064,47.0269445 C37.6877975,45.7413454 37.9267552,44.2010277 38.2186386,42.6678486 C38.3143485,42.1639275 38.5970412,41.8079704 39.0898519,41.6450802 C40.207629,41.2761438 41.0024646,40.5525481 41.4711894,39.4356799 C41.7567343,38.7555649 42.4808967,38.5517899 43.2050591,38.8950923 C44.4042847,39.4639098 45.5772059,39.414264 46.7010045,38.7017008 C46.9957401,38.514799 47.257833,38.2587824 47.4964738,37.9975741 C47.8504736,37.6101422 48.2567651,37.383978 48.7723941,37.5114995 C49.312109,37.6451863 49.6337828,38.0215858 49.7253727,38.5878075 C49.8759098,39.5148536 50.3836157,40.1466207 51.2545121,40.4217817 C52.1450575,40.7031079 52.9788743,40.5334037 53.59465,39.7961798 C53.8757581,39.4596916 54.0706639,39.0041183 54.1784167,38.5696365 C54.3153262,38.0173675 54.597702,37.6539473 55.1117464,37.5192871 C55.6324461,37.3830045 56.0593374,37.5806143 56.4073156,38.0150961 C57.3533221,39.1962119 59.1334621,39.6096024 60.5016061,38.9875697 C60.5821038,38.9509032 60.6622846,38.9132633 60.7424654,38.8756233 C61.4618739,38.5371882 62.1587812,38.7665973 62.4677783,39.5142046 C62.9121002,40.5885657 63.6948928,41.2495362 64.7404121,41.6226909 C65.2547734,41.8063479 65.52986,42.1629541 65.6461697,42.705813 C66.093027,44.7964528 66.2964897,46.9013699 66.1091899,49.041331 C65.930447,51.0787557 65.4103812,53.0250008 64.5733953,54.8709809 C63.9062785,56.3421839 63.1482057,57.7705552 62.4196064,59.2122303 C62.0041242,60.0341441 61.7268191,60.8696862 61.7097054,61.8233398 C61.6694566,64.0739494 61.0726961,66.1730258 59.831954,68.0524276 C58.2942578,70.3818864 56.1544134,71.7602874 53.4415776,72.1561559 C50.8288885,72.5377471 48.3996961,72.0224691 46.2462241,70.4046001 C44.6445101,69.2010951 43.5723695,67.5900402 42.8951113,65.691494 C42.4419155,64.42147 42.1652443,63.1053696 42.174435,61.7594168 C42.1817242,60.6967371 41.7706788,59.8138206 41.3333291,58.9111107 C40.5232814,57.2387287 39.6704495,55.5838687 38.9440687,53.8735222 C38.0604955,51.7926168 37.7011081,49.5825675 37.7198064,47.0269445'
                  id='face'
                  fill='#1F1F1F'
                ></path>
                <path
                  d='M52.8365333,65.4206605 C53.4071886,65.1681317 53.990884,64.9427737 54.5441527,64.654763 C55.0455719,64.3936035 55.5063187,64.4584938 55.7646349,64.9053739 C56.0080483,65.3266814 55.8906883,65.8672209 55.4209377,66.1060045 C54.7940861,66.4247023 54.147985,66.7293351 53.4789087,66.9294402 C51.7135406,67.4568738 50.0621174,67.0192638 48.4867608,66.1632231 C47.9667131,65.8806465 47.791915,65.3749496 48.0281874,64.9303071 C48.2771894,64.4616904 48.7550123,64.3904069 49.3144906,64.6723441 C49.8516145,64.9427737 50.4163708,65.1540668 51.0332871,65.4193819 C51.0593672,65.2077691 51.0832738,65.1019628 51.0835843,64.9961564 C51.0860681,64.0934458 51.0947614,63.1900959 51.0733386,62.2880247 C51.0696129,62.1246801 50.9755386,61.9188212 50.8563157,61.8114165 C49.9835669,61.025061 49.0928105,60.2594831 48.209816,59.4859139 C47.8322769,59.1547495 47.7748388,58.6404219 48.0679284,58.2673825 C48.3746789,57.8764423 48.8726828,57.8393621 49.2797172,58.1909845 C50.0838507,58.8843199 50.8889157,59.5770161 51.6787673,60.2869736 C51.8811978,60.4694976 52.0084931,60.4605472 52.2050246,60.2860147 C52.9883562,59.5878844 53.7841068,58.9044583 54.5783051,58.2200733 C55.0257014,57.8345673 55.4703034,57.8591808 55.8115168,58.2769722 C56.1278921,58.6640765 56.0738693,59.1333325 55.6441701,59.5121257 C54.7537242,60.296883 53.8521011,61.0682146 52.9607238,61.8520129 C52.8660285,61.935763 52.7468056,62.0649043 52.7449428,62.1748662 C52.7284875,63.2150292 52.735318,64.2555118 52.735318,65.2959944 C52.7688495,65.3375498 52.8030018,65.3791051 52.8365333,65.4206605'
                  id='nose'
                  fill='#1F1F1F'
                ></path>
                <path
                  d='M47.8818447,47.7071385 C47.8861849,48.8324547 46.9905958,49.7446363 45.8684959,49.7580851 C44.7565599,49.7712416 43.826123,48.8564288 43.8159591,47.7404682 C43.8057951,46.6107664 44.7341992,45.6620391 45.8478776,45.6640824 C46.9583616,45.6658399 47.8777633,46.5894237 47.8818447,47.7071385'
                  id='left_eye'
                  fill='#71CC98'
                ></path>
                <path
                  d='M57.0302734,47.7310415 C57.0273698,46.5920298 57.9482485,45.6597125 59.0722793,45.6640824 C60.1820487,45.6681908 61.0930318,46.5926145 61.0962417,47.7178855 C61.0994348,48.844326 60.1913623,49.7576403 59.0679136,49.7582253 C57.945047,49.7588097 57.0334818,48.8516349 57.0302734,47.7310415'
                  id='right_eye'
                  fill='#71CC98'
                ></path>
                <rect
                  id='closed-left'
                  fillOpacity='0'
                  fill='#71CC98'
                  x='43.8158764'
                  y='46.6876181'
                  width='4.06596832'
                  height='2.04707145'
                ></rect>
                <rect
                  id='closed-right'
                  fillOpacity='0'
                  fill='#71CC98'
                  x='57.0302734'
                  y='46.6876181'
                  width='4.06596832'
                  height='2.04707145'
                ></rect>
              </g>
            </g>
          </g>
        </svg>
      </div>
    </div>
  );
};
