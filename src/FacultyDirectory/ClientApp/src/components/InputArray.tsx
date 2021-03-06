import React, { useState, useEffect } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

export const InputArray = (props: any) => {
  const [values, setValues] = useState<string[]>([]);

  useEffect(() => {
    // update our value array each time the input data changes
    setValues(props.data);
  }, [props.data]);

  const setValuesAndNotifyProps = (values: string[]) => {
    setValues(values);

    const response = {
      target: { name: props.name, value: values.filter(v => !!v).join('|') }
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
    setValuesAndNotifyProps(values.filter((_, valIdx) => idx !== valIdx));
  };

  const onAdd = () => {
    setValuesAndNotifyProps([...values, '']);
  };

  return (
    <div className='input-array'>
      {values.map((val, idx) => (
        <div className='input-group' key={idx}>
          <input
            type='text'
            className='form-control'
            value={val}
            onChange={e => onChange(idx, e)}
          ></input>
          <div className='input-group-append'>
            <button type='button' className='btn' onClick={_ => onRemove(idx)}>
              <FontAwesomeIcon icon='times' />
            </button>
          </div>
        </div>
      ))}
      <button type='button' onClick={onAdd} className='btn addmore-btn'>
        <FontAwesomeIcon icon='plus' size='sm' />
        Add Another Item
      </button>
    </div>
  );
};
