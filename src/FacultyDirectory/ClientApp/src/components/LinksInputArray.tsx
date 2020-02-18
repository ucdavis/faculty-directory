import React, { useState, useEffect } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { ActiveIndicator } from './InputArray';

interface IProps {
    data: string;
    name: string;
    onChange: any; // TODO: update with real values and extract to use here and in input array
}
export const LinksInputArray = (props: any) => {
  const [values, setValues] = useState<string[]>([]);

  useEffect(() => {
    // update our value array each time the input data changes
    setValues(props.data);
  }, [props.data]);

  const setValuesAndNotifyProps = (values: string[]) => {
    setValues(values);

    const response = {
      target: {
        name: props.name,
        value: values.filter(v => !!v).join('|')
      }
    };

    // tell the parent about the new values
    props.onChange && props.onChange(response);
  };

  const onChange = (idx: number, e: any) => {
    // given the changed index and
    const newValues = values.map((val, valIdx) => {
      return idx === valIdx ? e.target.value : val;
    });

    setValuesAndNotifyProps(newValues);
  };

  const onRemove = (idx: number) => {
    const baseIndex = idx * 2;
    setValuesAndNotifyProps(values.filter((_, valIdx) => baseIndex !== valIdx && baseIndex + 1 !== valIdx));
  };

  const onAdd = () => {
      // add two inputs per new website
    setValuesAndNotifyProps([...values, '', '']);
  };

  const numGroups = values.length / 2;

  console.log('link array', numGroups, values);

  return (
    <div className='input-array'>
      {[...Array(numGroups)].map((_, i) => (
        <div className='input-group' key={`group-${i}`}>
          <ActiveIndicator hasValue={!!values[i * 2]} />
          <input
            type='text'
            className='form-control'
            placeholder='URL (ex: https://ucdavis.edu)'
            value={values[i * 2]}
            onChange={e => onChange(i * 2, e)}
          ></input>
          <input
            type='text'
            className='form-control'
            placeholder='Link text'
            value={values[i * 2 + 1]}
            onChange={e => onChange(i * 2 + 1, e)}
          ></input>
          <div className='input-group-append'>
            <button
              type='button'
              className='btn pop'
              onClick={_ => onRemove(i)}
            >
              <FontAwesomeIcon icon='times' size='2x' />
            </button>
          </div>
        </div>
      ))}
      <button type='button' onClick={onAdd} className='btn push'>
        <FontAwesomeIcon icon='plus' size='sm' />
        Add Another Website
      </button>
    </div>
  );
};
